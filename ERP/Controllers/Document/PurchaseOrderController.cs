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
                // ✅ Calculate only remaining quantity to receive
                var remainingToReceive = item.QuantityRequested - item.QuantityReceived;
                if (remainingToReceive <= 0)
                    continue;

                var newReceivedQty = remainingToReceive;

                // 1️⃣ Update ProductStock
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock == null)
                {
                    stock = new ProductStock
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        QuantityAvailable = newReceivedQty,
                        QuantityReserved = 0
                    };
                    _context.ProductStocks.Add(stock);
                }
                else
                {
                    stock.QuantityAvailable += newReceivedQty;
                }

                // 2️⃣ Update PO item
                item.QuantityReceived += newReceivedQty;
                item.QuantityRequested = Math.Max(0, item.QuantityRequested - newReceivedQty);

                // 3️⃣ Update linked Sales Order item
                if (item.SalesOrderItemId != Guid.Empty)
                {
                    var soItem = await _context.SalesOrderItems
                        .Include(s => s.SalesOrder)
                        .FirstOrDefaultAsync(s => s.Id == item.SalesOrderItemId);

                    if (soItem != null)
                    {
                        var remainingQty = soItem.QuantityRequested - soItem.QuantityFulfilled;
                        var fulfillQty = Math.Min(newReceivedQty, remainingQty);

                        soItem.QuantityFulfilled += fulfillQty;

                        // Track SalesOrderId for MRP & status update
                        salesOrderIds.Add(soItem.SalesOrderId);
                    }
                }

                // 4️⃣ Log stock transaction
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

            // 6️⃣ Update linked Sales Orders via MRP
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
