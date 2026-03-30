using ERP.Data;
using ERP.Entity;
using ERP.Entity.Document;
using ERP.Entity.DTO;
using ERP.Entity.Product;
using ERP.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductionController : ControllerBase
{
    private readonly ManufacturingDbContext _context;

    public ProductionController(ManufacturingDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-production-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderDto dto)
    {
        if (dto.ProductId == Guid.Empty)
            return BadRequest("ProductId is required.");

        if (dto.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return NotFound("Product not found.");

        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ProductId == dto.ProductId);

        if (bom == null || !bom.Items.Any())
            return BadRequest("No BOM defined for this product.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new ProductionOrder
            {
                OrderNumber = $"PROD-{Guid.NewGuid().ToString().Substring(0, 8)}",
                ProductId = dto.ProductId,
                BillOfMaterialId = bom.Id,
                PlannedQuantity = dto.Quantity,
                ProducedQuantity = 0,
                Status = nameof(ProductionStatus.Planned),
                PlannedStartDate = dto.StartDate,
                PlannedFinishDate = dto.FinishDate
            };

            _context.ProductionOrders.Add(order);
            await _context.SaveChangesAsync();

            var supplierOrders = new Dictionary<Guid, PurchaseOrder>();

            foreach (var item in bom.Items)
            {
                var requiredQty = item.Quantity * dto.Quantity;

                bool saved = false;
                int retry = 3;

                while (!saved && retry > 0)
                {
                    var stock = await _context.ProductStocks
                        .FirstOrDefaultAsync(s => s.ProductId == item.ComponentId);

                    if (stock == null)
                        throw new Exception($"Stock not found for component {item.ComponentId}");

                    var availableStock = stock.QuantityAvailable;

                    decimal qtyToReserve = 0;

                    if (availableStock >= requiredQty)
                    {
                        qtyToReserve = requiredQty;
                    }
                    else
                    {
                        qtyToReserve = availableStock > 0 ? availableStock : 0;
                    }

                    stock.QuantityAvailable -= qtyToReserve;
                    stock.QuantityReserved += qtyToReserve;

                    var shortageQty = requiredQty - qtyToReserve;

                    if (shortageQty > 0)
                    {
                        var supplier = await _context.ProductSuppliers
                            .Where(x => x.ProductId == item.ComponentId)
                            .OrderByDescending(x => x.IsPreferred)
                            .ThenBy(x => x.Price)
                            .FirstOrDefaultAsync();

                        if (supplier == null)
                            throw new Exception($"No supplier for component {item.ComponentId}");

                        if (!supplierOrders.ContainsKey(supplier.SupplierId))
                        {
                            supplierOrders[supplier.SupplierId] = new PurchaseOrder
                            {
                                Id = Guid.NewGuid(),
                                OrderNumber = $"PO-{Guid.NewGuid().ToString().Substring(0, 8)}",
                                SupplierId = supplier.SupplierId,
                                OrderDate = DateTime.UtcNow,
                                ExpectedDate = DateTime.UtcNow.AddDays(supplier.LeadTimeInDays),
                                Status = PurchaseOrderStatus.Draft,
                                Items = new List<PurchaseOrderItem>()
                            };
                        }

                        supplierOrders[supplier.SupplierId].Items.Add(new PurchaseOrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ComponentId,
                            Quantity = shortageQty,
                            UnitPrice = supplier.Price
                        });
                    }

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ComponentId,
                        Quantity = qtyToReserve,
                        Type = nameof(StockTransactionType.RESERVE),
                        ReferenceId = order.Id,
                        Date = DateTime.UtcNow,
                        Notes = $"Reserved {qtyToReserve} for Production Order {order.OrderNumber}",
                        PerformedBy = "SYSTEM"
                    });

                    try
                    {
                        await _context.SaveChangesAsync();
                        saved = true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        retry--;
                        if (retry == 0)
                            return Conflict($"Stock for component {item.ComponentId} was updated by another user. Please retry.");

                        // Reload stock and retry
                        await _context.Entry(stock).ReloadAsync();
                    }
                }
            }

            // ADD ALL GROUPED PURCHASE ORDERS
            foreach (var po in supplierOrders.Values)
            {
                _context.PurchaseOrders.Add(po);
            }

            // Add Production Operations
            var operations = new List<ProductionOperation>
        {
            new ProductionOperation
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OperationName = "Cutting",
                SequenceNumber = 1,
                Status = nameof(ProductionStatus.Pending)
            },
            new ProductionOperation
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OperationName = "Assembly",
                SequenceNumber = 2,
                Status = nameof(ProductionStatus.Pending)
            },
            new ProductionOperation
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OperationName = "Packaging",
                SequenceNumber = 3,
                Status = nameof(ProductionStatus.Pending)
            }
        };

            _context.ProductionOperations.AddRange(operations);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                order.ProductId,
                order.PlannedQuantity,
                order.Status
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("issue-material")]
    public async Task<IActionResult> IssueMaterialsForOrder(Guid orderId)
    {
        var order = await _context.ProductionOrders
            .Include(o => o.BillOfMaterials)
                .ThenInclude(b => b.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound("Order not found");

        if (order.Status != nameof(ProductionStatus.Planned))
            return BadRequest("Materials can only be issued for planned orders.");

        var bom = order.BillOfMaterials;

        if (bom == null || bom.Items == null || !bom.Items.Any())
            return BadRequest("No BOM defined for this product.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in bom.Items)
            {
                var requiredQty = item.Quantity * order.PlannedQuantity;
                bool saved = false;
                int retry = 3;

                while (!saved && retry > 0)
                {
                    var stock = await _context.ProductStocks
                        .FirstOrDefaultAsync(s => s.ProductId == item.ComponentId);

                    if (stock == null)
                        return BadRequest($"Stock record not found for material {item.ComponentId}");

                    if (stock.QuantityReserved < requiredQty)
                        return BadRequest($"Not enough reserved stock for material {item.ComponentId}");

                    // Move stock: Reserved → InProduction
                    stock.QuantityReserved -= requiredQty;
                    stock.QuantityInProduction += requiredQty;
                    stock.LastUpdated = DateTime.UtcNow;

                    // Record consumption
                    var existingConsumption = await _context.MaterialConsumptions
                        .FirstOrDefaultAsync(m => m.OrderId == orderId && m.MaterialId == item.ComponentId);

                    if (existingConsumption != null)
                    {
                        existingConsumption.ConsumedQuantity += requiredQty;
                        existingConsumption.PlannedQuantity = item.Quantity;
                        existingConsumption.ConsumptionDate = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.MaterialConsumptions.Add(new MaterialConsumption
                        {
                            Id = Guid.NewGuid(),
                            OrderId = orderId,
                            MaterialId = item.ComponentId,
                            PlannedQuantity = item.Quantity,
                            ConsumedQuantity = requiredQty,
                            ConsumptionDate = DateTime.UtcNow
                        });
                    }

                    // Stock transaction
                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ComponentId,
                        Quantity = requiredQty,
                        Type = nameof(StockTransactionType.ISSUE),
                        ReferenceId = orderId,
                        Date = DateTime.UtcNow,
                        Notes = $"Issued {requiredQty} units from Reserved → InProduction for Order {order.OrderNumber}",
                        PerformedBy = "SYSTEM"
                    });

                    try
                    {
                        await _context.SaveChangesAsync();
                        saved = true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        retry--;
                        if (retry == 0)
                            return Conflict($"Stock for material {item.ComponentId} was updated by another user. Please retry.");

                        // Reload stock for retry
                        await _context.Entry(stock).ReloadAsync();
                    }
                }
            }

            // Update order status
            order.Status = "Ready";

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Conflict(new
                {
                    Message = "Stock was modified by another user. Please retry."
                });
            }

            return Ok(new
            {
                Message = "Materials issued successfully",
                OrderId = order.Id,
                OrderStatus = order.Status
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("start-production")]
    public async Task<IActionResult> StartProduction(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return BadRequest("Invalid OrderId.");

        bool saved = false;
        int retry = 3;

        while (!saved && retry > 0)
        {
            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Production order not found.");

            if (order.Status == nameof(ProductionStatus.Completed))
                return BadRequest("Production already completed.");

            if (order.Status == nameof(ProductionStatus.InProgress))
                return BadRequest("Production already started.");

            if (order.Status != "Ready")
                return BadRequest("Materials must be issued before starting production.");

            var firstOperation = await _context.ProductionOperations
                .Where(o => o.OrderId == orderId)
                .OrderBy(o => o.SequenceNumber)
                .FirstOrDefaultAsync();

            if (firstOperation != null)
            {
                firstOperation.Status = nameof(ProductionStatus.InProgress);
            }

            order.Status = nameof(ProductionStatus.InProgress);
            order.ActualStartDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                saved = true;
            }
            catch (DbUpdateConcurrencyException)
            {
                retry--;
                if (retry == 0)
                    return Conflict(new
                    {
                        Message = "Production order was updated by another user. Please reload and retry."
                    });

                // Reload the entities for retry
                if (firstOperation != null)
                    await _context.Entry(firstOperation).ReloadAsync();

                await _context.Entry(order).ReloadAsync();
            }
        }

        return Ok(new
        {
            Message = "Production started successfully.",
            OrderId = orderId,
            Status = nameof(ProductionStatus.InProgress)
        });
    }

    [HttpPost("advance-production")]
    public async Task<IActionResult> AdvanceProduction(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return BadRequest("Invalid OrderId.");

        bool saved = false;
        int retry = 3;

        // Declare variables outside the loop
        string message = string.Empty;
        ProductionOperation currentOperation = null;
        ProductionOperation nextOperation = null;

        while (!saved && retry > 0)
        {
            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != nameof(ProductionStatus.InProgress))
                return BadRequest("Production is not in progress.");

            // Ensure only one operation is currently in progress
            currentOperation = await _context.ProductionOperations
                .Where(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.InProgress))
                .OrderBy(o => o.SequenceNumber)
                .FirstOrDefaultAsync();

            if (currentOperation == null)
                return BadRequest("No active operation found.");

            // Complete current operation
            currentOperation.Status = nameof(ProductionStatus.Completed);
            currentOperation.CompletedDate = DateTime.UtcNow;

            // Find the next pending operation
            nextOperation = await _context.ProductionOperations
                .Where(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.Pending))
                .OrderBy(o => o.SequenceNumber)
                .FirstOrDefaultAsync();

            if (nextOperation != null)
            {
                nextOperation.Status = nameof(ProductionStatus.InProgress);
                message = $"Operation '{currentOperation.OperationName}' completed. Next operation: '{nextOperation.OperationName}'.";
            }
            else
            {
                message = "All operations completed. Production ready for completion.";
            }

            try
            {
                await _context.SaveChangesAsync();
                saved = true;
            }
            catch (DbUpdateConcurrencyException)
            {
                retry--;
                if (retry == 0)
                    return Conflict(new
                    {
                        Message = "Production operation was updated by another user. Please reload and retry."
                    });

                // Reload entities for retry
                await _context.Entry(order).ReloadAsync();
                await _context.Entry(currentOperation).ReloadAsync();
                if (nextOperation != null)
                    await _context.Entry(nextOperation).ReloadAsync();
            }
        }

        // Return response using variables declared outside the loop
        var totalOperations = await _context.ProductionOperations.CountAsync(o => o.OrderId == orderId);
        var completedOperations = await _context.ProductionOperations.CountAsync(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.Completed));
        var progress = Math.Round((double)completedOperations / totalOperations * 100, 2);

        return Ok(new
        {
            Message = message,
            CurrentOperation = currentOperation.OperationName,
            NextOperation = nextOperation?.OperationName,
            CompletedOperations = completedOperations,
            TotalOperations = totalOperations,
            ProgressPercent = progress
        });
    }

    [HttpPost("complete-production")]
    public async Task<IActionResult> CompleteProduction(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return BadRequest("Invalid OrderId.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Production order not found.");

            if (order.Status != nameof(ProductionStatus.InProgress))
                return BadRequest("Production must be started before completion.");

            // Ensure all operations are completed
            var pendingOperations = await _context.ProductionOperations
                .Where(o => o.OrderId == orderId && o.Status != nameof(ProductionStatus.Completed))
                .AnyAsync();

            if (pendingOperations)
                return BadRequest("All operations must be completed first.");

            var producedQty = order.PlannedQuantity;

            // Component stock update (WIP → Consumed)
            var bomItems = await _context.BillOfMaterialItems
                .Where(a => a.BillOfMaterialId == order.BillOfMaterialId)
                .ToListAsync();

            foreach (var item in bomItems)
            {
                var requiredQty = item.Quantity * producedQty;

                var componentStock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ComponentId);

                if (componentStock == null)
                    return BadRequest($"Stock not found for component {item.ComponentId}");

                componentStock.QuantityInProduction -= requiredQty;
                componentStock.LastUpdated = DateTime.UtcNow;

                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ComponentId,
                    Quantity = requiredQty,
                    Type = nameof(StockTransactionType.CONSUME),
                    ReferenceId = order.Id,
                    Date = DateTime.UtcNow,
                    Notes = $"Consumed {requiredQty} units in production for Order {order.OrderNumber}",
                    PerformedBy = "SYSTEM"
                });
            }

            // Finished goods stock update
            var finishedStock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == order.ProductId);

            if (finishedStock == null)
                return BadRequest("Product stock record not found.");

            finishedStock.QuantityAvailable += producedQty;
            finishedStock.LastUpdated = DateTime.UtcNow;

            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = order.ProductId,
                Quantity = producedQty,
                Type = nameof(StockTransactionType.RECEIPT),
                ReferenceId = order.Id,
                Date = DateTime.UtcNow,
                Notes = $"Received {producedQty} units from production Order {order.OrderNumber}",
                PerformedBy = "SYSTEM"
            });

            // Update order
            order.ProducedQuantity = producedQty;
            order.Status = nameof(ProductionStatus.Completed);
            order.ActualFinishDate = DateTime.UtcNow;

            // Create finished goods receipt
            _context.FinishedGoodsReceipts.Add(new FinishedGoodsReceipt
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = producedQty,
                ReceiptDate = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync(); // EF will check RowVersion
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Conflict(new
                {
                    Message = "Concurrency conflict detected. Stock or order was modified by another user. Please retry."
                });
            }

            return Ok(new
            {
                Message = "Production completed successfully.",
                OrderId = order.Id,
                ProducedQuantity = order.ProducedQuantity,
                Status = order.Status
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
