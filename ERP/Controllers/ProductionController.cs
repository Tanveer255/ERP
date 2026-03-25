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

        // Check product exists
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return NotFound("Product not found.");

        // Get BOM header for this product
        var bom = await _context.BillOfMaterials
            .Include(b => b.Items) // Include BOM items
            .FirstOrDefaultAsync(b => b.ProductId == dto.ProductId);

        if (bom == null || bom.Items == null || !bom.Items.Any())
            return BadRequest("No BOM defined for this product.");

        // Begin transaction for atomic operation
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create production order
            var order = new ProductionOrder
            {
                OrderNumber = $"PO-{DateTime.UtcNow.Ticks}",
                ProductId = dto.ProductId,
                BillOfMaterialId = bom.Id,
                PlannedQuantity = dto.Quantity,
                ProducedQuantity = 0,
                Status = nameof(ProductionStatus.Planned),
                PlannedStartDate = dto.StartDate,
                PlannedFinishDate = dto.FinishDate
            };

            _context.ProductionOrders.Add(order);
            _context.SaveChanges(); // Save to get the generated OrderId

            // Reserve stock for each BOM item
            foreach (var item in bom.Items)
            {
                var requiredQty = item.Quantity * dto.Quantity;

                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ComponentId);

                if (stock == null)
                    return BadRequest($"Stock record not found for component {item.ComponentId}");

                if (stock.QuantityAvailable < requiredQty)
                {
                    var availableStock = stock.QuantityAvailable;

                    if (availableStock < requiredQty)
                    {
                        var shortageQty = requiredQty - availableStock;

                        //  Find supplier
                        var supplier = await _context.ProductSuppliers
                            .Where(x => x.ProductId == item.ComponentId)
                            .OrderByDescending(x => x.IsPreferred)
                            .ThenBy(x => x.Price)
                            .FirstOrDefaultAsync();

                        if (supplier == null)
                            return BadRequest($"No supplier found for component {item.ComponentId}");

                        // Create Purchase Order
                        var po = new PurchaseOrder
                        {
                            Id = Guid.NewGuid(),
                            OrderNumber = $"PO-{DateTime.UtcNow.Ticks}",
                            SupplierId = supplier.SupplierId,
                            OrderDate = DateTime.UtcNow,
                            ExpectedDate = DateTime.UtcNow.AddDays(supplier.LeadTimeInDays),
                            Status = PurchaseOrderStatus.Draft,
                            Items = new List<PurchaseOrderItem>()
                        };

                        po.Items.Add(new PurchaseOrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ComponentId,
                            Quantity = shortageQty,
                            UnitPrice = supplier.Price
                        });

                        _context.PurchaseOrders.Add(po);

                        // Reserve only available stock
                        stock.QuantityReserved += availableStock;
                        stock.QuantityAvailable = 0;
                    }
                    else
                    {
                        stock.QuantityAvailable -= requiredQty;
                        stock.QuantityReserved += requiredQty;
                    }
                }

                stock.QuantityAvailable -= requiredQty;
                stock.QuantityReserved += requiredQty;
                stock.LastUpdated = DateTime.UtcNow;

                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ComponentId,
                    Quantity = requiredQty,
                    Type = nameof(StockTransactionType.RESERVE),
                    ReferenceId = order.Id,
                    Date = DateTime.UtcNow,
                    Notes = $"Reserved {requiredQty} units (Available → Reserved) for Production Order {order.OrderNumber}",
                    PerformedBy = "SYSTEM"
                });
            }
            // Example: create operations (you can fetch from Routing table later)
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
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Conflict("Concurrency conflict...");
            }

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                order.ProductId,
                order.BillOfMaterialId,
                order.PlannedQuantity,
                order.Status,
                order.PlannedStartDate,
                order.PlannedFinishDate
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Error creating production order: {ex.Message}");
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

                //  CORRECT STOCK TRANSACTION
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ComponentId,
                    Quantity = requiredQty, // Positive (movement tracking)
                    Type = nameof(StockTransactionType.ISSUE),
                    ReferenceId = orderId,
                    Date = DateTime.UtcNow,
                    Notes = $"Issued {requiredQty} units from Reserved → InProduction for Order {order.OrderNumber}",
                    PerformedBy = "SYSTEM"
                });
            }

            // Update order status
            order.Status = "Ready";

            try
            {
                await _context.SaveChangesAsync();   //  EF checks RowVersion here
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

        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound("Production order not found.");

        if (order.Status == nameof(ProductionStatus.Completed))
            return BadRequest("Production already completed.");

        if (order.Status == nameof(ProductionStatus.InProgress))
            return BadRequest("Production already started.");

        //if (order.Status != "Planned")
        //    return BadRequest("Only planned orders can be started.");
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

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Production started successfully.",
            Order = order
        });
    }

    [HttpPost("advance-production")]
    public async Task<IActionResult> AdvanceProduction(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return BadRequest("Invalid OrderId.");

        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound("Order not found.");

        if (order.Status != nameof(ProductionStatus.InProgress))
            return BadRequest("Production is not in progress.");

        //  Ensure only one operation is currently in progress
        var currentOperation = await _context.ProductionOperations
            .Where(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.InProgress))
            .OrderBy(o => o.SequenceNumber) // always pick the correct in-progress step
            .FirstOrDefaultAsync();

        if (currentOperation == null)
            return BadRequest("No active operation found.");

        //  Complete current operation
        currentOperation.Status = nameof(ProductionStatus.Completed);
        currentOperation.CompletedDate = DateTime.UtcNow; // optional

        //  Find the next pending operation using SequenceNumber
        var nextOperation = await _context.ProductionOperations
            .Where(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.Pending))
            .OrderBy(o => o.SequenceNumber)
            .FirstOrDefaultAsync();

        string message;

        if (nextOperation != null)
        {
            nextOperation.Status = nameof(ProductionStatus.InProgress);
            message = $"Operation '{currentOperation.OperationName}' completed. Next operation: '{nextOperation.OperationName}'.";
        }
        else
        {
            message = $"All operations completed. Production ready for completion.";
        }

        //  Calculate progress
        var totalOperations = await _context.ProductionOperations.CountAsync(o => o.OrderId == orderId);
        var completedOperations = await _context.ProductionOperations.CountAsync(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.Completed));
        var progress = Math.Round((double)completedOperations / totalOperations * 100, 2);

        await _context.SaveChangesAsync();

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

            //  Ensure all operations are completed
            var pendingOperations = await _context.ProductionOperations
                .Where(o => o.OrderId == orderId && o.Status != nameof(ProductionStatus.Completed))
                .AnyAsync();

            if (pendingOperations)
                return BadRequest("All operations must be completed first.");

            var producedQty = order.PlannedQuantity;

            //  Update order
            order.ProducedQuantity = producedQty;
            order.Status = nameof(ProductionStatus.Completed);
            order.ActualFinishDate = DateTime.UtcNow;

            //  Component stock update (WIP → Consumed)
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

                //  Stock transaction (consume material)
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

            //  Finished goods stock update
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == order.ProductId);

            if (stock == null)
                return BadRequest("Product stock record not found.");

            stock.QuantityAvailable += producedQty;
            stock.LastUpdated = DateTime.UtcNow;

            //  Stock transaction (finished goods receipt)
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

            //  Create receipt record
            var receipt = new FinishedGoodsReceipt
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = producedQty,
                ReceiptDate = DateTime.UtcNow
            };

            _context.FinishedGoodsReceipts.Add(receipt);

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Conflict("Concurrency conflict...");
            }

            return Ok(new
            {
                Message = "Production completed successfully.",
                OrderId = order.Id,
                ProducedQuantity = order.ProducedQuantity,
                Status = order.Status
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();

            return Conflict(new
            {
                Message = "Concurrency conflict detected. The record was modified by another user. Please reload and try again."
            });
        }
    }
}
