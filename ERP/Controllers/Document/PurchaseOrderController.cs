using ERP.Data;
using ERP.Entity;
using ERP.Entity.Product;
using ERP.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers.Document;

[Route("api/[controller]")]
[ApiController]
public class PurchaseOrderController : ControllerBase
{
    private readonly ManufacturingDbContext _context;
    public PurchaseOrderController(ManufacturingDbContext context)
    {
        _context = context;
    }

    [HttpPost("receive-purchase-order")]
    public async Task<IActionResult> ReceivePurchaseOrder([FromQuery] Guid purchaseOrderId)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

        if (po == null)
            return NotFound(new { Message = "Purchase order not found." });

        foreach (var item in po.Items)
        {
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

            if (stock == null)
            {
                stock = new ProductStock
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    QuantityAvailable = item.Quantity,
                    QuantityReserved = 0
                };
                _context.ProductStocks.Add(stock);
            }
            else
            {
                // update stock
                stock.QuantityReserved -= item.Quantity;
                stock.QuantityAvailable += item.Quantity;

                //update purchase order item quantity
                item.Quantity -= item.Quantity;
            }

            // Log the stock transaction
            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Type = "RECEIVE",
                ReferenceId = po.Id,
                Date = DateTime.UtcNow,
                PerformedBy = "System",
                Notes = $"Received from Purchase Order {po.OrderNumber}"
            });
        }

        // Update PO status
        po.Status = PurchaseOrderStatus.Received;

        await _context.SaveChangesAsync();

        // Return updated PO info
        var result = new
        {
            po.Id,
            po.OrderNumber,
            po.Status,
            ReceivedDate = DateTime.UtcNow,
            Items = po.Items.Select(i => new
            {
                i.ProductId,
                i.Quantity
            })
        };

        return Ok(result);
    }
}
