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
        var po = await _purchaseOrderService.GetPurchaseOrderByIdAsync(purchaseOrderId);

        if (!po.IsSuccess)
            return NotFound(po.Message);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            //  Load SalesOrderItems via PO Items
            var salesOrderIds = new HashSet<Guid>();

            foreach (var item in po.Data.Items)
            {
                var receivedQty = item.QuantityRequested;

                if (receivedQty <= 0)
                    continue;

                //  1. UPDATE STOCK
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

                //  2. UPDATE PO ITEM
                item.QuantityReceived += receivedQty;
                item.QuantityRequested = 0;

                //  3. UPDATE LINKED SALES ORDER ITEM (CRITICAL)
                if (item.SalesOrderItemId != Guid.Empty)
                {
                    var soItem = await _context.SalesOrderItems
                        .Include(s => s.SalesOrder)
                        .FirstOrDefaultAsync(s => s.Id == item.SalesOrderItemId);

                    if (soItem != null)
                    {
                        var remainingQty = soItem.QuantityRequested - soItem.QuantityFulfilled;
                        var fulfillQty = Math.Min(receivedQty, remainingQty);

                        soItem.QuantityFulfilled += fulfillQty;

                        // Track SalesOrderId for later status update
                        salesOrderIds.Add(soItem.SalesOrderId);

                        // 🔥 ITEM STATUS
                        if (soItem.QuantityFulfilled == soItem.QuantityRequested)
                            soItem.Status = "Completed";
                        else
                            soItem.Status = "Partial";
                    }
                }

                //  4. LOG STOCK TRANSACTION (FIXED)
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = receivedQty, // ✅ FIXED
                    Type = "RECEIVE",
                    ReferenceId = po.Data.Id,
                    Date = DateTime.UtcNow,
                    PerformedBy = "System",
                    Notes = $"Received from PO {po.Data.OrderNumber}"
                });
            }

            //  5. UPDATE PURCHASE ORDER STATUS
            po.Data.Status = PurchaseOrderStatus.Received;

            //  6. UPDATE LINKED SALES ORDERS (IMPORTANT)
            if (salesOrderIds.Any())
            {
                var salesOrders = await _context.SalesOrders
                    .Include(o => o.Items)
                    .Where(o => salesOrderIds.Contains(o.Id))
                    .ToListAsync();

                foreach (var so in salesOrders)
                {
                    Helper.UpdateSalesOrderStatus(so);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                po.Data.Id,
                po.Data.OrderNumber,
                Status = po.Data.Status.ToString(),
                ReceivedDate = DateTime.UtcNow,
                Items = po.Data.Items.Select(i => new
                {
                    i.ProductId,
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
