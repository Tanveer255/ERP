using ERP.Data;
using ERP.Data.DTO;
using ERP.Data.DTO.Document;
using ERP.Data.DTO.MRP;
using ERP.Entity;
using ERP.Entity.Contact;
using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service;
using ERP.Service.Document;
using ERP.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers.Order;

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
            var order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"SO-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                Status = SalesOrderStatus.Pending,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                Items = new List<SalesOrderItem>()
            };

            decimal totalAmount = 0;

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
                    QuantityFulfilled = 0,
                    Status = nameof(SalesOrderStatus.Pending),
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            order.TotalAmount = totalAmount;

            await _context.SalesOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Trigger MRP (creates PO if needed)
            await _mrpService.RunMrpForSalesOrder(order.Id);

            await transaction.CommitAsync();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                Status = order.Status.ToString(),
                order.TotalAmount,
                Message = "Sales Order created. Waiting for stock."
            });
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

                if (stock == null)
                    continue;

                // =========================
                // 1️⃣ RESERVE (ONLY REMAINING)
                // =========================
                var reserveRemaining = item.QuantityRequested - item.QuantityReserved;

                if (reserveRemaining > 0 && stock.QuantityAvailable > 0)
                {
                    var reserveQty = Math.Min(reserveRemaining, stock.QuantityAvailable);

                    stock.QuantityAvailable -= reserveQty;
                    stock.QuantityReserved += reserveQty;

                    item.QuantityReserved += reserveQty;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = reserveQty,
                        Type = "RESERVE",
                        ReferenceId = order.Id,
                        Date = DateTime.UtcNow,
                        PerformedBy = "System",
                        Notes = $"Reserved for SO {order.OrderNumber}"
                    });
                }

                // =========================
                // 2️⃣ FULFILL (FROM RESERVED ONLY)
                // =========================
                var fulfillRemaining = item.QuantityReserved - item.QuantityFulfilled;

                if (fulfillRemaining > 0)
                {
                    var fulfillQty = fulfillRemaining;

                    item.QuantityFulfilled += fulfillQty;

                    // 🔥 CRITICAL FIX: RELEASE RESERVED
                    item.QuantityReserved -= fulfillQty;
                    if (item.QuantityReserved < 0)
                        item.QuantityReserved = 0;

                    stock.QuantityReserved -= fulfillQty;
                    if (stock.QuantityReserved < 0)
                        stock.QuantityReserved = 0;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = fulfillQty,
                        Type = "FULFILL",
                        ReferenceId = order.Id,
                        Date = DateTime.UtcNow,
                        PerformedBy = "System",
                        Notes = $"Fulfilled for SO {order.OrderNumber}"
                    });
                }

                // =========================
                // 3️⃣ ITEM STATUS
                // =========================
                if (item.QuantityFulfilled >= item.QuantityRequested)
                    item.Status = "Completed";
                else if (item.QuantityFulfilled > 0)
                    item.Status = "Partial";
                else
                    item.Status = "Pending";
            }

            // =========================
            // 4️⃣ ORDER STATUS (CENTRALIZED)
            // =========================
            Helper.UpdateSalesOrderStatus(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                OrderStatus = order.Status.ToString(),
                Items = order.Items.Select(i => new
                {
                    i.ProductId,
                    i.QuantityRequested,
                    i.QuantityReserved,
                    i.QuantityFulfilled,
                    i.Status
                })
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost("ship-sales-order")]
    public async Task<IActionResult> ShipSalesOrder([FromQuery] Guid salesOrderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.SalesOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == salesOrderId);

            if (order == null)
                return NotFound("Sales Order not found");

            foreach (var item in order.Items)
            {
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock == null)
                    continue;

                // Remaining quantity to ship
                var remainingToShip = item.QuantityRequested;
                if (remainingToShip <= 0)
                    continue;

                // Only ship from RESERVED stock
                var availableToShip = item.QuantityReserved;
                if (availableToShip <= 0)
                    continue;

                var shipQty = Math.Min(remainingToShip, availableToShip);

                // Deduct from ProductStock reserved quantity
                stock.QuantityReserved -= shipQty;
                _context.ProductStocks.Update(stock); // Ensure EF tracks the change

                // Update SO item quantities
                item.QuantityFulfilled += shipQty;
                item.QuantityReserved -= shipQty;

                // Optional: calculate item status immediately
                item.Status = item.QuantityFulfilled >= item.QuantityRequested ? "Completed"
                             : item.QuantityFulfilled > 0 ? "Partial"
                             : "Pending";

                // Log stock transaction
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = -shipQty,
                    Type = "SHIP",
                    ReferenceId = order.Id,
                    Date = DateTime.UtcNow,
                    PerformedBy = "System",
                    Notes = $"Shipped for SO {order.OrderNumber}"
                });
            }

            // Update total quantities at order level
            order.TotalQuantity = order.Items.Sum(i => i.QuantityRequested);
            order.TotalFulfilledQuantity = order.Items.Sum(i => i.QuantityFulfilled);

            // Update item statuses
            Helper.UpdateSalesOrderItemsStatus(order);

            // Update overall Sales Order status
            Helper.UpdateSalesOrderStatus(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                Status = order.Status.ToString(),
                Items = order.Items.Select(i => new
                {
                    i.ProductId,
                    i.QuantityRequested,
                    i.QuantityReserved,
                    i.QuantityFulfilled,
                    i.Status
                })
            });
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
}
