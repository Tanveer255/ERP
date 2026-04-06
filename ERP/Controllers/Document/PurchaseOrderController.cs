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
    public PurchaseOrderController(
        ManufacturingDbContext context,
        PurchaseOrderService purchaseOrderService,
        SalesOrderService salesOrderService
        )
    {
        _context = context;
        _purchaseOrderService = purchaseOrderService;
        _salesOrderService = salesOrderService;
    }

    [HttpPost("receive-purchase-order")]
    public async Task<IActionResult> ReceivePurchaseOrder([FromQuery] Guid purchaseOrderId)
    {
        var poResult = await _purchaseOrderService.GetPurchaseOrderByIdAsync(purchaseOrderId);

        if (!poResult.IsSuccess)
            return NotFound(poResult.Message);

        var po = poResult.Data;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var salesOrderIds = new HashSet<Guid>();

            foreach (var item in po.Items)
            {
                // =========================
                //  FIX 1: CALCULATE PENDING QTY
                // =========================
                var pendingQty = item.QuantityRequested - item.QuantityReceived;

                if (pendingQty <= 0)
                    continue; // already fully received → skip

                var receivedQty = pendingQty; // full receive (can be partial later)

                // =========================
                // 1️⃣ UPDATE STOCK
                // =========================
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock == null)
                {
                    stock = new ProductStock
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        QuantityAvailable = receivedQty,
                        QuantityReserved = 0
                    };
                    _context.ProductStocks.Add(stock);
                }
                else
                {
                    stock.QuantityAvailable += receivedQty;
                }

                // =========================
                // 2️⃣ UPDATE PO ITEM
                // =========================
                item.QuantityReceived += receivedQty;

                // =========================
                // 3️⃣ UPDATE SALES ORDER ITEM
                // =========================
                if (item.SalesOrderItemId != Guid.Empty)
                {
                    var soItem = await _context.SalesOrderItems
                        .Include(s => s.SalesOrder)
                        .FirstOrDefaultAsync(s => s.Id == item.SalesOrderItemId);

                    if (soItem != null)
                    {
                        var remainingQty = soItem.QuantityRequested - soItem.QuantityFulfilled;

                        if (remainingQty > 0)
                        {
                            var fulfillQty = Math.Min(receivedQty, remainingQty);

                            soItem.QuantityFulfilled += fulfillQty;

                            // reduce receivedQty (safe allocation)
                            receivedQty -= fulfillQty;

                            salesOrderIds.Add(soItem.SalesOrderId);
                        }

                        // =========================
                        // 🔥 FIXED ITEM STATUS
                        // =========================
                        if (soItem.QuantityFulfilled >= soItem.QuantityRequested)
                            soItem.Status = "Completed";
                        else if (soItem.QuantityFulfilled > 0)
                            soItem.Status = "Partial";
                        else
                            soItem.Status = "Pending";
                    }
                }

                // =========================
                // 4️⃣ STOCK TRANSACTION LOG
                // =========================
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = receivedQty,
                    Type = "RECEIVE",
                    ReferenceId = po.Id,
                    Date = DateTime.UtcNow,
                    PerformedBy = "System",
                    Notes = $"Received from PO {po.OrderNumber}"
                });
            }

            // =========================
            // 5️⃣ UPDATE PO STATUS
            // =========================
            if (po.Items.All(i => i.QuantityReceived >= i.QuantityRequested))
                po.Status = PurchaseOrderStatus.Received;
            else
                po.Status = PurchaseOrderStatus.PartiallyReceived;

            po.ExpectedDate = DateTime.UtcNow;

            // =========================
            // 6️⃣ UPDATE SALES ORDERS
            // =========================
            if (salesOrderIds.Any())
            {
                var salesOrders = await _context.SalesOrders
                    .Include(o => o.Items)
                    .Where(o => salesOrderIds.Contains(o.Id))
                    .ToListAsync();

                foreach (var so in salesOrders)
                {
                    Helper.UpdateSalesOrderStatus(so);
                    _context.SalesOrders.Update(so);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                po.Id,
                po.OrderNumber,
                Status = po.Status.ToString(),
                ReceivedDate = DateTime.UtcNow,
                Items = po.Items.Select(i => new
                {
                    i.ProductId,
                    i.QuantityRequested,
                    i.QuantityReceived
                })
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
