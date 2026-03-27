using ERP.Data;
using ERP.Entity.Document;
using ERP.Entity;
using ERP.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity.DTO.Document;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Product;

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
            // 1️⃣ Create Sales Order
            var order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"SO-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                Status = SalesOrderStatus.Confirmed,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                Items = new List<SalesOrderItem>()
            };

            decimal totalAmount = 0;

            // Grouped Purchase Orders by Supplier
            var purchaseOrders = new Dictionary<Guid, PurchaseOrder>();

            foreach (var item in dto.Items)
            {
                // 2️⃣ Get Product
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

                var availableQty = stock?.QuantityAvailable ?? 0;

                // correct reservation calculation
                var reservedQty = Math.Min(item.Quantity, availableQty);
                var shortage = item.Quantity - reservedQty;

                // 3️⃣ Apply Reservation (ONLY AVAILABLE)
                if (reservedQty > 0 && stock != null)
                {
                    stock.QuantityAvailable -= reservedQty;
                    stock.QuantityReserved += reservedQty;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = reservedQty,
                        Type = "RESERVE",
                        ReferenceId = order.Id,
                        Date = DateTime.UtcNow,
                        PerformedBy = dto.CustomerName,
                        Notes = $"Reserved {reservedQty} for Sales Order {order.OrderNumber}"
                    });
                }

                // 4️⃣ Handle Shortage
                if (shortage > 0)
                {
                    var bom = await _context.BillOfMaterials
                        .Include(b => b.Items)
                        .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

                    if (bom == null)
                    {
                        // PURCHASE FLOW
                        var productSupplier = await _context.ProductSuppliers
                            .Include(ps => ps.Supplier)
                            .Where(ps => ps.ProductId == item.ProductId && ps.Supplier.IsActive)
                            .OrderByDescending(ps => ps.IsPreferred)
                            .FirstOrDefaultAsync();

                        if (productSupplier == null)
                            return BadRequest($"No supplier found for Product {item.ProductId}");

                        var supplierId = productSupplier.SupplierId;

                        if (!purchaseOrders.ContainsKey(supplierId))
                        {
                            purchaseOrders[supplierId] = new PurchaseOrder
                            {
                                Id = Guid.NewGuid(),
                                OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                                SupplierId = supplierId,
                                OrderDate = DateTime.UtcNow,
                                ExpectedDate = DateTime.UtcNow.AddDays(
                                    productSupplier.LeadTimeInDays > 0
                                        ? productSupplier.LeadTimeInDays
                                        : 3),
                                Status = PurchaseOrderStatus.Pending,
                                Items = new List<PurchaseOrderItem>(),
                                Notes = $"Auto-created for Sales Order {order.OrderNumber}"
                            };
                        }

                        var po = purchaseOrders[supplierId];

                        var poItem = new PurchaseOrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ProductId,
                            Quantity = shortage,
                            UnitPrice = productSupplier.Price,
                            TotalPrice = productSupplier.Price * shortage
                        };

                        po.Items.Add(poItem);
                        po.SubTotal += poItem.TotalPrice;
                        po.TotalAmount += poItem.TotalPrice;
                    }
                    else
                    {
                        // PRODUCTION FLOW
                        var productionOrder = new ProductionOrder
                        {
                            Id = Guid.NewGuid(),
                            OrderNumber = $"AUTO-PROD-{DateTime.UtcNow.Ticks}",
                            ProductId = item.ProductId,
                            BillOfMaterialId = bom.Id,
                            PlannedQuantity = shortage,
                            ProducedQuantity = 0,
                            Status = nameof(ProductionStatus.Planned),
                            PlannedStartDate = DateTime.UtcNow,
                            PlannedFinishDate = DateTime.UtcNow.AddDays(2)
                        };

                        _context.ProductionOrders.Add(productionOrder);

                        // Handle BOM Materials
                        foreach (var bomItem in bom.Items)
                        {
                            var requiredQty = bomItem.Quantity * shortage;

                            var materialStock = await _context.ProductStocks
                                .FirstOrDefaultAsync(s => s.ProductId == bomItem.ComponentId);

                            var availableMaterial = materialStock?.QuantityAvailable ?? 0;
                            var reserveMaterial = Math.Min(requiredQty, availableMaterial);
                            var materialShortage = requiredQty - reserveMaterial;

                            // Reserve material
                            if (reserveMaterial > 0 && materialStock != null)
                            {
                                materialStock.QuantityAvailable -= reserveMaterial;
                                materialStock.QuantityReserved += reserveMaterial;

                                _context.StockTransactions.Add(new StockTransaction
                                {
                                    Id = Guid.NewGuid(),
                                    ProductId = bomItem.ComponentId,
                                    Quantity = reserveMaterial,
                                    Type = "RESERVE",
                                    ReferenceId = productionOrder.Id,
                                    Date = DateTime.UtcNow,
                                    PerformedBy = dto.CustomerName,
                                    Notes = $"Reserved for Production {productionOrder.OrderNumber}"
                                });
                            }

                            // Create PO for material shortage
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
                                            ExpectedDate = DateTime.UtcNow.AddDays(3),
                                            Status = PurchaseOrderStatus.Pending,
                                            Items = new List<PurchaseOrderItem>(),
                                            Notes = $"Auto-created for Production {productionOrder.OrderNumber}"
                                        };
                                    }

                                    var po = purchaseOrders[supplier.SupplierId];

                                    var poItem = new PurchaseOrderItem
                                    {
                                        Id = Guid.NewGuid(),
                                        ProductId = bomItem.ComponentId,
                                        Quantity = materialShortage,
                                        UnitPrice = supplier.Price,
                                        TotalPrice = supplier.Price * materialShortage
                                    };

                                    po.Items.Add(poItem);
                                    po.SubTotal += poItem.TotalPrice;
                                    po.TotalAmount += poItem.TotalPrice;
                                }
                            }
                        }
                    }
                }

                // 5️⃣ Add Sales Order Item 
                order.Items.Add(new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    RequestedQuantity = item.Quantity,
                    ReservedQuantity = reservedQty,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            // 6️⃣ Save Purchase Orders
            foreach (var po in purchaseOrders.Values)
            {
                _context.PurchaseOrders.Add(po);
            }

            order.TotalAmount = totalAmount;

            // 7️⃣ Save Sales Order
            await _context.SalesOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();


            var response = await _context.SalesOrders
                 .Where(o => o.Id == order.Id)
                 .Select(o => new SalesOrderResponseDto
                 {
                     Id = o.Id,
                     OrderNumber = o.OrderNumber,
                     OrderDate = o.OrderDate,
                     CustomerName = o.CustomerName,
                     CustomerEmail = o.CustomerEmail,
                     TotalAmount = o.TotalAmount,
                     Status = o.Status,

                     Items = o.Items.Select(i => new SalesOrderItemResponseDto
                     {
                         ProductId = i.ProductId,
                         ProductName = i.Product.Name, // if navigation exists
                         ReservedQuantity = i.ReservedQuantity,
                     }).ToList()
                 })
                 .FirstOrDefaultAsync();
            return Ok(response);
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
            foreach (var item in order.Items)
            {
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock == null || stock.QuantityAvailable <= 0)
                    continue;

                //  FIXED calculation
                var remainingQty = item.RequestedQuantity - item.ReservedQuantity;

                if (remainingQty <= 0)
                    continue;

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
                    PerformedBy = "System",
                    Notes = $"Reserved {qtyToReserve} units for Sales Order {order.OrderNumber}"
                });
            }

            //  FIXED STATUS LOGIC
            if (order.Items.All(i => i.ReservedQuantity == i.RequestedQuantity))
            {
                order.Status = SalesOrderStatus.Confirmed;
            }
            else if (order.Items.Any(i => i.ReservedQuantity > 0))
            {
                order.Status = SalesOrderStatus.PartiallyReserved;
            }
            else
            {
                order.Status = SalesOrderStatus.Pending;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            //  UPDATED RESPONSE (NO EMPTY ITEMS, SHOW SHORTAGE)
            var response = await _context.SalesOrders
                .Where(o => o.Id == order.Id)
                .Select(o => new SalesOrderResponseDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,

                    Items = o.Items.Select(i => new SalesOrderItemResponseDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,

                        RequestedQuantity = i.RequestedQuantity,
                        ReservedQuantity = i.ReservedQuantity,

                        //  KEY FIELD
                        ShortQuantity = i.RequestedQuantity - i.ReservedQuantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
