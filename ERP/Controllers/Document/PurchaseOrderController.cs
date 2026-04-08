using ERP.Data;
using ERP.Entity;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service;
using ERP.Service.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers.Document;

[Route("api/[controller]")]
[ApiController]
public class PurchaseOrderController : ControllerBase
{
    private readonly ManufacturingDbContext _context;
    private readonly PurchaseOrderService _purchaseOrderService;
    private readonly SalesOrderService _salesOrderService;
    private readonly MrpService _mrpService;
    public PurchaseOrderController(
        ManufacturingDbContext context,
        PurchaseOrderService purchaseOrderService,
        SalesOrderService salesOrderService,
        MrpService mrpService
        )
    {
        _context = context;
        _purchaseOrderService = purchaseOrderService;
        _salesOrderService = salesOrderService;
        _mrpService = mrpService;
    }

    [HttpPost("receive-purchase-order")]
    public async Task<IActionResult> ReceivePurchaseOrder([FromQuery] Guid purchaseOrderId)
    {
        var po = await _purchaseOrderService.GetPurchaseOrderByIdAsync(purchaseOrderId);

        if (!po.IsSuccess)
            return NotFound(po.Message);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var salesOrderIds = new HashSet<Guid>();

            foreach (var item in po.Data.Items)
            {
                // Remaining qty to receive (DELTA SAFE)
                var remainingToReceive = item.QuantityRequested - item.QuantityReceived;
                if (remainingToReceive <= 0)
                    continue;

                var newReceivedQty = remainingToReceive;

                // 1️⃣ Get or create stock
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

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

                // Add to available stock
                stock.QuantityAvailable += newReceivedQty;

                // 2️⃣ Update PO item
                item.QuantityReceived += newReceivedQty;

                // 3️⃣ Handle linked Sales Order
                if (item.SalesOrderItemId != Guid.Empty)
                {
                    var soItem = await _context.SalesOrderItems
                        .Include(s => s.SalesOrder)
                        .FirstOrDefaultAsync(s => s.Id == item.SalesOrderItemId);

                    if (soItem != null)
                    {
                        // STEP A: Reserve stock first
                        var reserveRemaining = soItem.QuantityRequested - soItem.QuantityReserved;

                        if (reserveRemaining > 0)
                        {
                            var reserveQty = Math.Min(stock.QuantityAvailable, reserveRemaining);

                            stock.QuantityAvailable -= reserveQty;
                            stock.QuantityReserved += reserveQty;

                            soItem.QuantityReserved += reserveQty;
                        }

                        // STEP B: Fulfill from reserved stock
                        var fulfillRemaining = soItem.QuantityRequested - soItem.QuantityFulfilled;

                        if (fulfillRemaining > 0)
                        {
                            var fulfillQty = Math.Min(soItem.QuantityReserved - soItem.QuantityFulfilled, fulfillRemaining);

                            soItem.QuantityFulfilled += fulfillQty;
                        }

                        salesOrderIds.Add(soItem.SalesOrderId);
                    }
                }

                // 4️⃣ Stock transaction log
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = newReceivedQty,
                    Type = "RECEIVE",
                    ReferenceId = po.Data.Id,
                    Date = DateTime.UtcNow,
                    PerformedBy = "System",
                    Notes = $"Received from PO {po.Data.OrderNumber}"
                });
            }

            // 5️⃣ Update PO status
            po.Data.Status = PurchaseOrderStatus.Received;
            po.Data.ExpectedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 6️⃣ Run MRP (important for other pending SOs)
            if (salesOrderIds.Any())
            {
                foreach (var soId in salesOrderIds)
                {
                    await _mrpService.RunMrpForSalesOrder(soId);
                }
            }

            await transaction.CommitAsync();

            return Ok(new
            {
                po.Data.Id,
                po.Data.OrderNumber,
                Status = po.Data.Status.ToString(),
                ReceivedDate = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
