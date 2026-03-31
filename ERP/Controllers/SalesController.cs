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
    private readonly ILogger<SalesController> _logger;
    private readonly BillOfMaterialService _bomService;

    public SalesController(ManufacturingDbContext context, ProductService productService, ILogger<SalesController> logger, BillOfMaterialService bomService)
    {
        _context = context;
        _productService = productService;
        _logger = logger;
        _bomService = bomService;
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

                var totalPrice = unitPrice * item.QuantityRequested;
                totalAmount += totalPrice;

                var salesOrderItem = new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    QuantityRequested = item.QuantityRequested,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                };

                order.Items.Add(salesOrderItem);

                //  Reserve stock
                var reservedQty = await ReserveStockAsync(
                    item.ProductId,
                    item.QuantityRequested,
                    salesOrderItem,
                    order.Id,
                    dto.CustomerName);

                var shortage = item.QuantityRequested - reservedQty;

                if (shortage <= 0) continue;

                //  Check BOM
                var bomResult = await _bomService.GetBillOfMaterialByProductId(item.ProductId);

                if (bomResult.Data == null)
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
                        BillOfMaterialId = bomResult.Data.Id,

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
                    foreach (var bomItem in bomResult.Data.Items)
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
                ReservedItems = order.Items.Count(i => i.QuantityReserved >= i.QuantityRequested),
                PendingItems = order.Items.Count(i => i.QuantityReserved < i.QuantityRequested),

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
                        QuantityPlanned = p.PlannedQuantity
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
            bool stockAvailable = false;

            foreach (var item in order.Items)
            {
                // 1️⃣ Check actual stock in warehouse
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                var availableStock = stock != null ? stock.QuantityAvailable : 0;

                // 2️⃣ Include pending received POs
                var pendingPOItems = await _context.PurchaseOrderItems
                    .Include(poi => poi.PurchaseOrder)
                    .Where(poi => poi.ProductId == item.ProductId &&
                                  poi.PurchaseOrder.Status != PurchaseOrderStatus.Received)
                    .OrderBy(poi => poi.PurchaseOrder.OrderDate)
                    .ToListAsync();

                // Auto-receive pending POs to fulfill this sales order
                foreach (var poItem in pendingPOItems)
                {
                    var poQtyToReceive = poItem.QuantityRequested - poItem.QuantityReceived;
                    if (poQtyToReceive <= 0)
                        continue;

                    // Create stock if not exists
                    if (stock == null)
                    {
                        stock = new ProductStock
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ProductId,
                            QuantityAvailable = 0,
                            QuantityReserved = 0
                        };
                        _context.ProductStocks.Add(stock);
                    }

                    // Receive from PO
                    stock.QuantityAvailable += poQtyToReceive;


                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = poQtyToReceive,
                        Type = "RECEIVE",
                        ReferenceId = poItem.PurchaseOrderId,
                        Date = DateTime.UtcNow,
                        PerformedBy = "System",
                        Notes = $"Auto-received {poQtyToReceive} units from PO {poItem.PurchaseOrder.OrderNumber} to fulfill SO {order.OrderNumber}"
                    });

                    poItem.QuantityReceived += poQtyToReceive;

                    // If all items in PO are received, mark PO as received
                    if (poItem.QuantityReceived >= poItem.QuantityRequested)
                    {
                        poItem.PurchaseOrder.Status = PurchaseOrderStatus.Received;
                        poItem.QuantityRequested -= poItem.QuantityRequested;
                    }
                }

                // 3️⃣ Recalculate available stock
                availableStock = stock?.QuantityAvailable ?? 0;
                var remainingQty = item.QuantityRequested - item.QuantityReserved;
                if (remainingQty <= 0 || availableStock <= 0)
                    continue;

                // 4️⃣ Reserve stock for the sales order
                var qtyToReserve = Math.Min(remainingQty, availableStock);
                stock.QuantityAvailable -= qtyToReserve;
                stock.QuantityReserved += qtyToReserve;
                item.QuantityReserved += qtyToReserve;
                item.QuantityRequested -= qtyToReserve;

                if (item.QuantityReserved > 0)
                    stockAvailable = true;

                // Log stock transaction for reservation
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

            // 5️⃣ Update reservation status for sales order
            Helper.UpdateReservationStatus(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 6️⃣ Prepare pending purchase orders
            var pendingPurchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Items)
                .Where(po => po.Items.Any(i => i.SalesOrderItem.SalesOrderId == order.Id) &&
                             po.Status != PurchaseOrderStatus.Received)
                .Select(po => new PurchaseOrderSummaryDto
                {
                    PurchaseOrderId = po.Id,
                    SupplierId = po.SupplierId,
                    OrderNumber = po.OrderNumber,
                    TotalItems = po.Items.Count,
                    Status = po.Status.ToString(),
                    PendingItems = po.Items.Sum(i => i.QuantityRequested - i.QuantityReceived)
                }).ToListAsync();

            // 7️⃣ Prepare pending production orders
            var pendingProductionOrders = await _context.ProductionOrders
                .Where(po => po.SalesOrderItem.SalesOrderId == order.Id &&
                             po.Status != nameof(ProductionStatus.Completed))
                .Select(po => new ProductionOrderSummaryDto
                {
                    ProductionOrderId = po.Id,
                    OrderNumber = po.OrderNumber,
                    ProductId = po.ProductId,
                    QuantityPlanned = po.PlannedQuantity,
                    QuantityProduced = po.ProducedQuantity,
                    Status = po.Status,
                    PlannedStartDate = po.PlannedStartDate,
                    PlannedFinishDate = po.PlannedFinishDate,
                    ActualStartDate = po.ActualStartDate,
                    ActualFinishDate = po.ActualFinishDate
                }).ToListAsync();

            // 8️⃣ Build response
            var response = new CreateSalesOrderResponseDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                TotalItems = order.Items.Count,
                ReservedItems = order.Items.Count(i => i.QuantityReserved >= i.QuantityRequested),
                PendingItems = order.Items.Count(i => i.QuantityReserved < i.QuantityRequested),
                IsStockAvailable = stockAvailable,
                Message = stockAvailable
                    ? "Stock reserved successfully. Pending POs auto-received if needed."
                    : "Order cannot proceed. Stock is pending from Purchase Order(s).",
                PurchaseOrders = pendingPurchaseOrders,
                ProductionOrders = pendingProductionOrders
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

        item.QuantityReserved = reservedQty;

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
            existing.QuantityRequested += quantity;
            existing.TotalPrice += quantity * existing.UnitPrice;
        }
        else
        {
            po.Items.Add(new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                QuantityRequested = quantity,
                UnitPrice = price,
                TotalPrice = price * quantity,
                SalesOrderItemId = salesOrderItemId
            });
        }
    }
}
