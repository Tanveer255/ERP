using ERP.Data;
using ERP.Entity;
using ERP.Entity.Product;
using ERP.Enum;
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

        foreach (var item in po.Data.Items)
        {
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

            if (stock == null)
            {
                stock = new ProductStock
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    QuantityAvailable = item.QuantityRequested,
                    QuantityReserved = 0
                };
                _context.ProductStocks.Add(stock);
            }
            else
            {
                // update stock
                //stock.QuantityReserved -= item.QuantityRequested;
                stock.QuantityAvailable += item.QuantityRequested;

                //update purchase order item quantity
                item.QuantityRequested -= item.QuantityRequested;
                item.QuantityReceived += item.QuantityRequested;
            }

            // Log the stock transaction
            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.QuantityRequested,
                Type = "RECEIVE",
                ReferenceId = po.Data.Id,
                Date = DateTime.UtcNow,
                PerformedBy = "System",
                Notes = $"Received from Purchase Order {po.Data.OrderNumber}"
            });
        }

        // Update PO status
        po.Data.Status = PurchaseOrderStatus.Received;

        await _context.SaveChangesAsync();
        //_salesOrderService.UpdateSalesOrderStock(po.Data.Id).Wait();

        // Return updated PO info
        var result = new
        {
            po.Data.Id,
            po.Data.OrderNumber,
            po.Data.Status,
            ReceivedDate = DateTime.UtcNow,
            Items = po.Data.Items.Select(i => new
            {
                i.ProductId,
                i.QuantityReceived
            })
        };

        return Ok(result);
    }
}
