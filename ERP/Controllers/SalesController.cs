using ERP.Data;
using ERP.Entity;
using ERP.Entity.Contact;
using ERP.Entity.Document;
using ERP.Entity.DTO;
using ERP.Entity.DTO.Document;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service;
using ERP.Service.Document;
using ERP.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly ManufacturingDbContext _context;
    private readonly ProductService _productService;
    private readonly ILogger<SalesController> _logger;
    private readonly BillOfMaterialService _bomService;
    private readonly MrpService _mrpService;

    public SalesController(ManufacturingDbContext context,
        ProductService productService,
        ILogger<SalesController> logger,
        BillOfMaterialService bomService,
        MrpService mrpService)
    {
        _context = context;
        _productService = productService;
        _logger = logger;
        _bomService = bomService;
        _mrpService = mrpService;
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
            // 1️⃣ Create Sales Order (ONLY DEMAND)
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

            // 2️⃣ Add Items (NO STOCK / NO PO / NO PRODUCTION)
            foreach (var item in dto.Items)
            {
                var result = await _productService.GetProductById(item.ProductId);
                if (!result.IsSuccess)
                    return BadRequest(result.Message);

                var product = result.Data;

                var unitPrice = product.Prices.FirstOrDefault()?.SalePrice ?? 0;
                var totalPrice = unitPrice * item.QuantityRequested;

                totalAmount += totalPrice;

                order.Items.Add(new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    QuantityRequested = item.QuantityRequested,
                    QuantityReserved = 0,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            order.TotalAmount = totalAmount;

            // 3️⃣ Save Sales Order
            await _context.SalesOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            // 4️⃣ Run MRP (PLANNING ONLY)
            await _mrpService.RunMrpForSalesOrder(order.Id);

            await transaction.CommitAsync();

            // 5️⃣ Response (NO RESERVATION YET)
            var response = new CreateSalesOrderResponseDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,

                TotalItems = order.Items.Count,
                ReservedItems = 0,
                PendingItems = order.Items.Count,

                IsStockAvailable = false,
                Message = "Sales Order created. Waiting for stock, purchase, or production."
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
                // 1️⃣ Get stock
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock == null || stock.QuantityAvailable <= 0)
                    continue;

                // 2️⃣ Calculate remaining quantity
                var remainingQty = item.QuantityRequested - item.QuantityReserved;
                if (remainingQty <= 0)
                    continue;

                // 3️⃣ Reserve stock (ONLY from available stock)
                var qtyToReserve = Math.Min(remainingQty, stock.QuantityAvailable);

                stock.QuantityAvailable -= qtyToReserve;
                stock.QuantityReserved += qtyToReserve;

                item.QuantityReserved += qtyToReserve;

                if (item.QuantityReserved > 0)
                    stockAvailable = true;

                // 4️⃣ Log transaction
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

            // 5️⃣ Update order reservation status
            Helper.UpdateReservationStatus(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 6️⃣ Pending Purchase Orders (DO NOT MODIFY THEM)
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

            // 7️⃣ Pending Production Orders
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

            // 8️⃣ Response
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
                    ? "Stock reserved successfully."
                    : "Stock not available. Waiting for Purchase or Production.",

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

    [HttpPost("run-mrp")]
    public async Task<IActionResult> RunMrp([FromQuery] Guid salesOrderId)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == salesOrderId);

        if (order == null) return NotFound("Sales order not found.");

        var mrpPlans = new List<MrpPlan>();

        foreach (var item in order.Items)
        {
            // 1️⃣ Stock in warehouse
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

            var availableStock = stock?.QuantityAvailable ?? 0;

            // 2️⃣ Pending receipts from POs
            var pendingPOQty = await _context.PurchaseOrderItems
                .Where(poi => poi.ProductId == item.ProductId &&
                              poi.PurchaseOrder.Status != PurchaseOrderStatus.Received)
                .SumAsync(poi => poi.QuantityRequested - poi.QuantityReceived);

            var totalAvailable = availableStock + pendingPOQty;

            var netRequirement = item.QuantityRequested - totalAvailable;

            if (netRequirement <= 0) continue; // Stock sufficient

            // 3️⃣ Create MRP plan
            var plan = new MrpPlan
            {
                Id = Guid.NewGuid(),
                SalesOrderId = order.Id,
                ProductId = item.ProductId,
                RequiredQuantity = item.QuantityRequested,
                AvailableQuantity = totalAvailable,
                PlannedQuantity = netRequirement,
                RequiredDate = order.OrderDate.AddDays(1), // example: next day
                Notes = $"Material needed for Sales Order {order.OrderNumber}"
            };

            mrpPlans.Add(plan);
        }

        if (mrpPlans.Any())
        {
            await _context.MrpPlans.AddRangeAsync(mrpPlans);
            await _context.SaveChangesAsync();
        }

        return Ok(mrpPlans.Select(p => new MrpPlanDto
        {
            ProductId = p.ProductId,
            RequiredQuantity = p.RequiredQuantity,
            AvailableQuantity = p.AvailableQuantity,
            PlannedQuantity = p.PlannedQuantity,
            RequiredDate = p.RequiredDate,
            Notes = p.Notes
        }));
    }

    // ================================
    // COMMON METHODS
    // ================================

    //private async Task<decimal> ReserveStockAsync(
    // Guid productId,
    // decimal quantity,
    // SalesOrderItem item,
    // Guid orderId,
    // string performedBy)
    //{
    //    var stock = await _context.ProductStocks
    //        .FirstOrDefaultAsync(s => s.ProductId == productId);

    //    if (stock == null || stock.QuantityAvailable <= 0)
    //        return 0;

    //    var reservedQty = Math.Min(quantity, stock.QuantityAvailable);

    //    stock.QuantityAvailable -= reservedQty;
    //    stock.QuantityReserved += reservedQty;

    //    item.QuantityReserved = reservedQty;

    //    _context.StockTransactions.Add(new StockTransaction
    //    {
    //        Id = Guid.NewGuid(),
    //        ProductId = productId,
    //        Quantity = reservedQty,
    //        Type = "RESERVE",
    //        ReferenceId = orderId,
    //        Date = DateTime.UtcNow,
    //        PerformedBy = performedBy
    //    });

    //    return reservedQty;
    //}

    //private async Task<ProductSupplier?> GetPreferredSupplierAsync(Guid productId)
    //{
    //    return await _context.ProductSuppliers
    //        .Include(ps => ps.Supplier)
    //        .Where(ps => ps.ProductId == productId && ps.Supplier.IsActive)
    //        .OrderByDescending(ps => ps.IsPreferred)
    //        .FirstOrDefaultAsync();
    //}

    //private PurchaseOrder GetOrCreatePurchaseOrder(
    //    Dictionary<Guid, PurchaseOrder> purchaseOrders,
    //    Guid supplierId)
    //{
    //    if (!purchaseOrders.ContainsKey(supplierId))
    //    {
    //        purchaseOrders[supplierId] = new PurchaseOrder
    //        {
    //            Id = Guid.NewGuid(),
    //            OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
    //            SupplierId = supplierId,
    //            OrderDate = DateTime.UtcNow,
    //            Status = PurchaseOrderStatus.Draft,
    //            Items = new List<PurchaseOrderItem>()
    //        };
    //    }

    //    return purchaseOrders[supplierId];
    //}

    //private void AddOrUpdatePurchaseOrderItem(
    //    PurchaseOrder po,
    //    Guid productId,
    //    decimal quantity,
    //    decimal price,
    //    Guid salesOrderItemId)
    //{
    //    var existing = po.Items.FirstOrDefault(x =>
    //        x.ProductId == productId &&
    //        x.SalesOrderItemId == salesOrderItemId);

    //    if (existing != null)
    //    {
    //        existing.QuantityRequested += quantity;
    //        existing.TotalPrice += quantity * existing.UnitPrice;
    //    }
    //    else
    //    {
    //        po.Items.Add(new PurchaseOrderItem
    //        {
    //            Id = Guid.NewGuid(),
    //            ProductId = productId,
    //            QuantityRequested = quantity,
    //            UnitPrice = price,
    //            TotalPrice = price * quantity,
    //            SalesOrderItemId = salesOrderItemId
    //        });
    //    }
    //}
}
