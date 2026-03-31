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
using ERP.Service.Product;

namespace ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly ManufacturingDbContext _context;
    private readonly ProductService _productService;

    public SalesController(ManufacturingDbContext context, ProductService productService)
    {
        _context = context;
        _productService = productService;
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
            var order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"SO-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                Status = SalesOrderStatus.Draft,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                Items = new List<SalesOrderItem>()
            };

            decimal totalAmount = 0;
            var purchaseOrders = new Dictionary<Guid, PurchaseOrder>();
            var productionOrders = new List<ProductionOrder>();

            foreach (var item in dto.Items)
            {
                //  Get product
                var result = await _productService.GetProductById(item.ProductId);
                if (!result.IsSuccess)
                    throw new Exception(result.Message);

                var product = result.Data;

                var unitPrice = product?.Prices?
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault()?.SalePrice ?? 0;

                var totalPrice = unitPrice * item.Quantity;
                totalAmount += totalPrice;

                var salesOrderItem = new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    RequestedQuantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                };

                order.Items.Add(salesOrderItem);

                //  Reserve stock
                var reservedQty = await ReserveStockAsync(
                    item.ProductId,
                    item.Quantity,
                    salesOrderItem,
                    order.Id,
                    dto.CustomerName);

                var shortage = item.Quantity - reservedQty;

                if (shortage <= 0) continue;

                //  Check BOM
                var bom = await _context.BillOfMaterials
                    .Include(b => b.Items)
                    .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

                if (bom == null)
                {
                    //  PURCHASE
                    var supplier = await GetPreferredSupplierAsync(item.ProductId);
                    if (supplier == null)
                        throw new Exception($"No supplier found for product {item.ProductId}");

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
                    //  PRODUCTION
                    var productionOrder = new ProductionOrder
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = $"MO-{DateTime.UtcNow.Ticks}",

                        ProductId = item.ProductId,
                        BillOfMaterialId = bom.Id,

                        PlannedQuantity = shortage,
                        ProducedQuantity = 0,

                        Status = nameof(ProductionStatus.Planned),

                        PlannedStartDate = DateTime.UtcNow,
                        PlannedFinishDate = DateTime.UtcNow.AddDays(2),

                        SalesOrderItemId = salesOrderItem.Id
                    };

                    _context.ProductionOrders.Add(productionOrder);
                    productionOrders.Add(productionOrder);

                    //  Handle BOM materials
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

            //  Save Purchase Orders
            foreach (var po in purchaseOrders.Values)
                _context.PurchaseOrders.Add(po);

            //  Update totals
            order.TotalAmount = totalAmount;

            Helper.UpdateReservationStatus(order);

            //  Save everything
            await _context.SalesOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            //  Response DTO
            var response = new CreateSalesOrderResponseDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,

                TotalItems = order.Items.Count,
                ReservedItems = order.Items.Count(i => i.ReservedQuantity >= i.RequestedQuantity),
                PendingItems = order.Items.Count(i => i.ReservedQuantity < i.RequestedQuantity),

                PurchaseOrders = purchaseOrders.Values
                    .Select(po => new PurchaseOrderSummaryDto
                    {
                        SupplierId = po.SupplierId,
                        TotalItems = po.Items.Count
                    }).ToList(),

                ProductionOrders = productionOrders
                    .Select(p => new ProductionOrderSummaryDto
                    {
                        ProductId = p.ProductId,
                        PlannedQuantity = p.PlannedQuantity
                    }).ToList()
            };

            return Ok(response);
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
