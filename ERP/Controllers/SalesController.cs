using ERP.Data;
using ERP.Entity.Document;
using ERP.Entity;
using ERP.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity.DTO.Document;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Product;
using ERP.Service;

namespace ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly ManufacturingDbContext _context;

    public SalesController(ManufacturingDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-sales-order")]
    public async Task<IActionResult> CreateSalesOrder(CreateSalesOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest("Order must have at least one item.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"SO-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                Status = SalesOrderStatus.Pending, // ✅ start as Pending
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                Items = new List<SalesOrderItem>()
            };

            decimal totalAmount = 0;
            var purchaseOrders = new Dictionary<Guid, PurchaseOrder>();

            foreach (var item in dto.Items)
            {
                var product = await _context.Products
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                    return BadRequest($"Product {item.ProductId} not found.");

                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                var unitPrice = product.Prices.FirstOrDefault()?.SalePrice ?? 0;
                var totalPrice = unitPrice * item.Quantity;
                totalAmount += totalPrice;

                // ✅ CREATE ITEM FIRST
                var salesOrderItem = new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    RequestedQuantity = item.Quantity,
                    ReservedQuantity = 0,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                };

                order.Items.Add(salesOrderItem);

                var availableQty = stock?.QuantityAvailable ?? 0;
                var reservedQty = Math.Min(item.Quantity, availableQty);
                var shortage = item.Quantity - reservedQty;

                // ✅ RESERVE STOCK
                if (reservedQty > 0 && stock != null)
                {
                    stock.QuantityAvailable -= reservedQty;
                    stock.QuantityReserved += reservedQty;

                    salesOrderItem.ReservedQuantity = reservedQty;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = reservedQty,
                        Type = "RESERVE",
                        ReferenceId = order.Id,
                        Date = DateTime.UtcNow,
                        PerformedBy = dto.CustomerName
                    });
                }

                // ✅ HANDLE SHORTAGE
                if (shortage > 0)
                {
                    var bom = await _context.BillOfMaterials
                        .Include(b => b.Items)
                        .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

                    if (bom == null)
                    {
                        // 🔵 PURCHASE
                        var supplier = await _context.ProductSuppliers
                            .Include(ps => ps.Supplier)
                            .Where(ps => ps.ProductId == item.ProductId && ps.Supplier.IsActive)
                            .OrderByDescending(ps => ps.IsPreferred)
                            .FirstOrDefaultAsync();

                        if (supplier == null)
                            return BadRequest("No supplier found.");

                        if (!purchaseOrders.ContainsKey(supplier.SupplierId))
                        {
                            purchaseOrders[supplier.SupplierId] = new PurchaseOrder
                            {
                                Id = Guid.NewGuid(),
                                OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                                SupplierId = supplier.SupplierId,
                                OrderDate = DateTime.UtcNow,
                                Status = PurchaseOrderStatus.Pending,
                                Items = new List<PurchaseOrderItem>()
                            };
                        }

                        var po = purchaseOrders[supplier.SupplierId];

                        // ✅ Prevent over-purchase + duplicates
                        var alreadyOrdered = po.Items
                            .Where(x => x.SalesOrderItemId == salesOrderItem.Id &&
                                        x.ProductId == item.ProductId)
                            .Sum(x => x.Quantity);

                        var remainingToOrder = shortage - alreadyOrdered;

                        if (remainingToOrder > 0)
                        {
                            var existingPoItem = po.Items
                                .FirstOrDefault(x =>
                                    x.ProductId == item.ProductId &&
                                    x.SalesOrderItemId == salesOrderItem.Id);

                            if (existingPoItem != null)
                            {
                                existingPoItem.Quantity += remainingToOrder;
                                existingPoItem.TotalPrice += remainingToOrder * existingPoItem.UnitPrice;
                            }
                            else
                            {
                                po.Items.Add(new PurchaseOrderItem
                                {
                                    Id = Guid.NewGuid(),
                                    ProductId = item.ProductId,
                                    Quantity = remainingToOrder,
                                    UnitPrice = supplier.Price,
                                    TotalPrice = supplier.Price * remainingToOrder,
                                    SalesOrderItemId = salesOrderItem.Id
                                });
                            }
                        }
                    }
                    else
                    {
                        // 🔵 PRODUCTION
                        var productionOrder = new ProductionOrder
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ProductId,
                            BillOfMaterialId = bom.Id,
                            PlannedQuantity = shortage,
                            Status = nameof(ProductionStatus.Planned),

                            // 🔗 LINK
                            SalesOrderItemId = salesOrderItem.Id
                        };

                        _context.ProductionOrders.Add(productionOrder);

                        foreach (var bomItem in bom.Items)
                        {
                            var requiredQty = bomItem.Quantity * shortage;

                            var materialStock = await _context.ProductStocks
                                .FirstOrDefaultAsync(s => s.ProductId == bomItem.ComponentId);

                            var availableMaterial = materialStock?.QuantityAvailable ?? 0;
                            var materialShortage = requiredQty - availableMaterial;

                            if (materialShortage > 0)
                            {
                                var supplier = await _context.ProductSuppliers
                                    .Include(ps => ps.Supplier)
                                    .Where(ps => ps.ProductId == bomItem.ComponentId && ps.Supplier.IsActive)
                                    .OrderByDescending(ps => ps.IsPreferred)
                                    .FirstOrDefaultAsync();

                                if (supplier != null)
                                {
                                    if (!purchaseOrders.ContainsKey(supplier.SupplierId))
                                    {
                                        purchaseOrders[supplier.SupplierId] = new PurchaseOrder
                                        {
                                            Id = Guid.NewGuid(),
                                            OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                                            SupplierId = supplier.SupplierId,
                                            OrderDate = DateTime.UtcNow,
                                            Status = PurchaseOrderStatus.Pending,
                                            Items = new List<PurchaseOrderItem>()
                                        };
                                    }

                                    var po = purchaseOrders[supplier.SupplierId];

                                    var alreadyOrdered = po.Items
                                        .Where(x => x.SalesOrderItemId == salesOrderItem.Id &&
                                                    x.ProductId == bomItem.ComponentId)
                                        .Sum(x => x.Quantity);

                                    var remainingToOrder = materialShortage - alreadyOrdered;

                                    if (remainingToOrder > 0)
                                    {
                                        var existingPoItem = po.Items
                                            .FirstOrDefault(x =>
                                                x.ProductId == bomItem.ComponentId &&
                                                x.SalesOrderItemId == salesOrderItem.Id);

                                        if (existingPoItem != null)
                                        {
                                            existingPoItem.Quantity += remainingToOrder;
                                            existingPoItem.TotalPrice += remainingToOrder * existingPoItem.UnitPrice;
                                        }
                                        else
                                        {
                                            po.Items.Add(new PurchaseOrderItem
                                            {
                                                Id = Guid.NewGuid(),
                                                ProductId = bomItem.ComponentId,
                                                Quantity = remainingToOrder,
                                                UnitPrice = supplier.Price,
                                                TotalPrice = supplier.Price * remainingToOrder,
                                                SalesOrderItemId = salesOrderItem.Id
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var po in purchaseOrders.Values)
                _context.PurchaseOrders.Add(po);

            order.TotalAmount = totalAmount;
            Helper.UpdateOrderStatus(order);
            await _context.SalesOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("update-sales-order-stock")]
    public async Task<IActionResult> UpdateSalesOrderStock([FromQuery] Guid salesOrderId)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == salesOrderId);

        if (order == null)
            return NotFound("Sales order not found.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in order.Items.Where(i => i.RequestedQuantity > i.ReservedQuantity))
            {
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock == null || stock.QuantityAvailable <= 0)
                    continue;

                var remainingQty = item.RequestedQuantity - item.ReservedQuantity;
                var qtyToReserve = Math.Min(remainingQty, stock.QuantityAvailable);

                stock.QuantityAvailable -= qtyToReserve;
                stock.QuantityReserved += qtyToReserve;

                item.ReservedQuantity += qtyToReserve;

                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = qtyToReserve,
                    Type = "RESERVE",
                    ReferenceId = order.Id,
                    Date = DateTime.UtcNow,
                    PerformedBy = "System"
                });
            }

            // Update order status
            Helper.UpdateOrderStatus(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Fetch prices for all items in one query to avoid multiple DB hits
            var productIds = order.Items.Select(i => i.ProductId).ToList();

            // Prepare response
            var response = new SalesOrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Status = order.Status,
                Items = order.Items.Select(i =>
                {
                    return new SalesOrderItemResponseDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        RequestedQuantity = i.RequestedQuantity,
                        ReservedQuantity = i.ReservedQuantity,
                        ShortQuantity = i.RequestedQuantity - i.ReservedQuantity,
                        Price = i.UnitPrice,
                        Total = i.UnitPrice * i.RequestedQuantity
                    };
                }).ToList()
            };

            // Calculate order total dynamically
            response.TotalAmount = response.Items.Sum(x => x.Total);

            return Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
