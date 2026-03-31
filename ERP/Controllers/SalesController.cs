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
using ERP.Entity.Contact;

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

    // ================================
    // CREATE SALES ORDER
    // ================================
    [HttpPost("create-sales-order")]
    public async Task<IActionResult> CreateSalesOrder(CreateSalesOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest("Order must have at least one item.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1️⃣ Create new Sales Order

            var order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"SO-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                Status = SalesOrderStatus.Draft, // ✅ Always start as Draft
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                Items = new List<SalesOrderItem>()
            };

            decimal totalAmount = 0;
            var purchaseOrders = new Dictionary<Guid, PurchaseOrder>();

            // 2️⃣ Process each order item
            foreach (var item in dto.Items)
            {
                // Fetch product with prices
                var product = await _context.Products
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                    return BadRequest($"Product {item.ProductId} not found.");

                var unitPrice = product.Prices.FirstOrDefault()?.SalePrice ?? 0;
                var totalPrice = unitPrice * item.Quantity;
                totalAmount += totalPrice;

                // Create SalesOrderItem
                var salesOrderItem = new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    RequestedQuantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                };

                order.Items.Add(salesOrderItem);

                // 3️⃣ Reserve stock if available
                var reservedQty = await ReserveStockAsync(
                    item.ProductId,
                    item.Quantity,
                    salesOrderItem,
                    order.Id,
                    dto.CustomerName);

                var shortage = item.Quantity - reservedQty;

                if (shortage <= 0) continue;

                // 4️⃣ Check for BOM (production) or purchase
                var bom = await _context.BillOfMaterials
                    .Include(b => b.Items)
                    .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

                if (bom == null)
                {
                    //  PURCHASE for shortage
                    var supplier = await GetPreferredSupplierAsync(item.ProductId);
                    if (supplier == null)
                        return BadRequest("No supplier found.");

                    var po = GetOrCreatePurchaseOrder(purchaseOrders, supplier.SupplierId);

                    AddOrUpdatePurchaseOrderItem(
                        po,
                        item.ProductId,
                        shortage,
                        supplier.Price,
                        salesOrderItem.Id);
                }
                else
                {
                    //  PRODUCTION for shortage
                    var productionOrder = new ProductionOrder
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        BillOfMaterialId = bom.Id,
                        PlannedQuantity = shortage,
                        Status = nameof(ProductionStatus.Planned),
                        SalesOrderItemId = salesOrderItem.Id
                    };

                    _context.ProductionOrders.Add(productionOrder);

                    // Reserve components or create purchase orders for materials
                    foreach (var bomItem in bom.Items)
                    {
                        var requiredQty = bomItem.Quantity * shortage;

                        var materialStock = await _context.ProductStocks
                            .FirstOrDefaultAsync(s => s.ProductId == bomItem.ComponentId);

                        var available = materialStock?.QuantityAvailable ?? 0;
                        var materialShortage = requiredQty - available;

                        if (materialShortage <= 0) continue;

                        var supplier = await GetPreferredSupplierAsync(bomItem.ComponentId);
                        if (supplier == null) continue;

                        var po = GetOrCreatePurchaseOrder(purchaseOrders, supplier.SupplierId);

                        AddOrUpdatePurchaseOrderItem(
                            po,
                            bomItem.ComponentId,
                            materialShortage,
                            supplier.Price,
                            salesOrderItem.Id);
                    }
                }
            }

            // 5️⃣ Add all generated purchase orders
            foreach (var po in purchaseOrders.Values)
                _context.PurchaseOrders.Add(po);
            // 6️⃣ Update totals and reservation status
            order.TotalAmount = totalAmount;

            // Update reservation only (do not change main order status)
            Helper.UpdateReservationStatus(order);

            // 7️⃣ Save order and commit
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

    // ================================
    // COMMON METHODS
    // ================================

    private async Task<decimal> ReserveStockAsync(
     Guid productId,
     decimal quantity,
     SalesOrderItem item,
     Guid orderId,
     string performedBy)
    {
        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (stock == null || stock.QuantityAvailable <= 0)
            return 0;

        var reservedQty = Math.Min(quantity, stock.QuantityAvailable);

        stock.QuantityAvailable -= reservedQty;
        stock.QuantityReserved += reservedQty;

        item.ReservedQuantity = reservedQty;

        _context.StockTransactions.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = reservedQty,
            Type = "RESERVE",
            ReferenceId = orderId,
            Date = DateTime.UtcNow,
            PerformedBy = performedBy
        });

        return reservedQty;
    }

    private async Task<ProductSupplier?> GetPreferredSupplierAsync(Guid productId)
    {
        return await _context.ProductSuppliers
            .Include(ps => ps.Supplier)
            .Where(ps => ps.ProductId == productId && ps.Supplier.IsActive)
            .OrderByDescending(ps => ps.IsPreferred)
            .FirstOrDefaultAsync();
    }

    private PurchaseOrder GetOrCreatePurchaseOrder(
        Dictionary<Guid, PurchaseOrder> purchaseOrders,
        Guid supplierId)
    {
        if (!purchaseOrders.ContainsKey(supplierId))
        {
            purchaseOrders[supplierId] = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                SupplierId = supplierId,
                OrderDate = DateTime.UtcNow,
                Status = PurchaseOrderStatus.Draft,
                Items = new List<PurchaseOrderItem>()
            };
        }

        return purchaseOrders[supplierId];
    }

    private void AddOrUpdatePurchaseOrderItem(
        PurchaseOrder po,
        Guid productId,
        decimal quantity,
        decimal price,
        Guid salesOrderItemId)
    {
        var existing = po.Items.FirstOrDefault(x =>
            x.ProductId == productId &&
            x.SalesOrderItemId == salesOrderItemId);

        if (existing != null)
        {
            existing.Quantity += quantity;
            existing.TotalPrice += quantity * existing.UnitPrice;
        }
        else
        {
            po.Items.Add(new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = price,
                TotalPrice = price * quantity,
                SalesOrderItemId = salesOrderItemId
            });
        }
    }
}
