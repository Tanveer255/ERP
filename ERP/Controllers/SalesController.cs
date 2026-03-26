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
            // 1️⃣ Create the sales order
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

            // 2️⃣ Prepare PurchaseOrders grouped by Supplier
            var purchaseOrders = new Dictionary<Guid, PurchaseOrder>();

            foreach (var item in dto.Items)
            {
                // Fetch product and stock
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

                // 3️⃣ Check stock
                if (stock == null || stock.QuantityAvailable < item.Quantity)
                {
                    var shortage = item.Quantity - (stock?.QuantityAvailable ?? 0);

                    // 3a️⃣ Check if product has BOM (manufactured item)
                    var bom = await _context.BillOfMaterials
                        .Include(b => b.Items)
                        .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

                    if (bom == null)
                    {
                        // Purchased product: create PurchaseOrder
                        var productSupplier = await _context.ProductSuppliers
                            .Include(ps => ps.Supplier)
                            .Where(ps => ps.ProductId == item.ProductId && ps.Supplier.IsActive)
                            .OrderByDescending(ps => ps.IsPreferred)
                            .FirstOrDefaultAsync();

                        if (productSupplier == null)
                            return BadRequest($"No active supplier found for Product {item.ProductId}");

                        var supplierId = productSupplier.SupplierId;

                        if (!purchaseOrders.ContainsKey(supplierId))
                        {
                            purchaseOrders[supplierId] = new PurchaseOrder
                            {
                                Id = Guid.NewGuid(),
                                OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                                SupplierId = supplierId,
                                OrderDate = DateTime.UtcNow,
                                ExpectedDate = DateTime.UtcNow.AddDays(productSupplier.LeadTimeInDays > 0
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

                        // Reserve available stock if any
                        if (stock != null && stock.QuantityAvailable > 0)
                        {
                            var reserveQty = stock.QuantityAvailable;
                            stock.QuantityAvailable -= reserveQty;
                            stock.QuantityReserved += reserveQty;

                            _context.StockTransactions.Add(new StockTransaction
                            {
                                Id = Guid.NewGuid(),
                                ProductId = item.ProductId,
                                Quantity = reserveQty,
                                Type = "RESERVE",
                                ReferenceId = order.Id,
                                Date = DateTime.UtcNow,
                                PerformedBy = dto.CustomerName,
                                Notes = $"Partial reserve for Sales Order {order.OrderNumber}"
                            });
                        }

                        continue;
                    }

                    // 3b️⃣ Manufactured product: create ProductionOrder
                    var productionOrder = new ProductionOrder
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                        ProductId = item.ProductId,
                        BillOfMaterialId = bom.Id,
                        PlannedQuantity = shortage,
                        ProducedQuantity = 0,
                        Status = nameof(ProductionStatus.Planned),
                        PlannedStartDate = DateTime.UtcNow,
                        PlannedFinishDate = DateTime.UtcNow.AddDays(2)
                    };

                    _context.ProductionOrders.Add(productionOrder);

                    // 4️⃣ Check BOM materials
                    foreach (var bomItem in bom.Items)
                    {
                        var requiredQty = bomItem.Quantity * shortage;

                        var materialStock = await _context.ProductStocks
                            .FirstOrDefaultAsync(s => s.ProductId == bomItem.ComponentId);

                        var componentProduct = await _context.Products
                            .Include(p => p.Prices)
                            .FirstOrDefaultAsync(p => p.Id == bomItem.ComponentId);

                        if (componentProduct == null)
                            continue;

                        var availableQty = materialStock?.QuantityAvailable ?? 0;

                        if (availableQty < requiredQty)
                        {
                            var materialShortage = requiredQty - availableQty;

                            // Create grouped PurchaseOrder for material
                            var productSupplier = await _context.ProductSuppliers
                                .Include(ps => ps.Supplier)
                                .Where(ps => ps.ProductId == bomItem.ComponentId && ps.Supplier.IsActive)
                                .OrderByDescending(ps => ps.IsPreferred)
                                .FirstOrDefaultAsync();

                            if (productSupplier != null)
                            {
                                var supplierId = productSupplier.SupplierId;

                                if (!purchaseOrders.ContainsKey(supplierId))
                                {
                                    purchaseOrders[supplierId] = new PurchaseOrder
                                    {
                                        Id = Guid.NewGuid(),
                                        OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                                        SupplierId = supplierId,
                                        OrderDate = DateTime.UtcNow,
                                        ExpectedDate = DateTime.UtcNow.AddDays(productSupplier.LeadTimeInDays > 0
                                            ? productSupplier.LeadTimeInDays
                                            : 3),
                                        Status = PurchaseOrderStatus.Pending,
                                        Items = new List<PurchaseOrderItem>(),
                                        Notes = $"Auto-created for Production {productionOrder.OrderNumber}"
                                    };
                                }

                                var po = purchaseOrders[supplierId];

                                var costPrice = componentProduct.Prices.FirstOrDefault()?.CostPrice ?? 0;

                                var poItem = new PurchaseOrderItem
                                {
                                    Id = Guid.NewGuid(),
                                    ProductId = bomItem.ComponentId,
                                    Quantity = materialShortage,
                                    UnitPrice = costPrice,
                                    TotalPrice = costPrice * materialShortage
                                };

                                po.Items.Add(poItem);
                                po.SubTotal += poItem.TotalPrice;
                                po.TotalAmount += poItem.TotalPrice;
                            }
                        }

                        // Reserve available material
                        if (materialStock != null && materialStock.QuantityAvailable > 0)
                        {
                            var reserveQty = Math.Min(materialStock.QuantityAvailable, requiredQty);
                            materialStock.QuantityAvailable -= reserveQty;
                            materialStock.QuantityReserved += reserveQty;

                            _context.StockTransactions.Add(new StockTransaction
                            {
                                Id = Guid.NewGuid(),
                                ProductId = bomItem.ComponentId,
                                Quantity = reserveQty,
                                Type = "RESERVE",
                                ReferenceId = productionOrder.Id,
                                Date = DateTime.UtcNow,
                                PerformedBy = dto.CustomerName,
                                Notes = $"Reserved for Production {productionOrder.OrderNumber}"
                            });
                        }
                    }

                    // Reserve available finished goods
                    if (stock != null && stock.QuantityAvailable > 0)
                    {
                        var reserveQty = stock.QuantityAvailable;
                        stock.QuantityAvailable -= reserveQty;
                        stock.QuantityReserved += reserveQty;

                        _context.StockTransactions.Add(new StockTransaction
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ProductId,
                            Quantity = reserveQty,
                            Type = "RESERVE",
                            ReferenceId = order.Id,
                            Date = DateTime.UtcNow,
                            PerformedBy = dto.CustomerName,
                            Notes = $"Partial reserve for Sales Order {order.OrderNumber}"
                        });
                    }
                }
                else
                {
                    // 5️⃣ Full reserve if stock sufficient
                    stock.QuantityAvailable -= item.Quantity;
                    stock.QuantityReserved += item.Quantity;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Type = "RESERVE",
                        ReferenceId = order.Id,
                        Date = DateTime.UtcNow,
                        PerformedBy = dto.CustomerName,
                        Notes = $"Reserved for Sales Order {order.OrderNumber}"
                    });
                }

                // 6️⃣ Add sales order item
                order.Items.Add(new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            // 7️⃣ Save PurchaseOrders
            foreach (var po in purchaseOrders.Values)
            {
                _context.PurchaseOrders.Add(po);
            }

            order.TotalAmount = totalAmount;

            // 8️⃣ Save SalesOrder
            _context.SalesOrders.Add(order);

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
}
