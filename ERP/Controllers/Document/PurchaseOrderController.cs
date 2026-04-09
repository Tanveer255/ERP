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
        var poResult = await _purchaseOrderService.GetPurchaseOrderByIdAsync(purchaseOrderId);
        if (!poResult.IsSuccess) return NotFound(poResult.Message);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var affectedSalesOrderIds = new HashSet<Guid>();
            var purchaseOrder = poResult.Data;

            foreach (var line in purchaseOrder.Items)
            {
                var qtyToReceive = line.QuantityRequested - line.QuantityReceived;
                if (qtyToReceive <= 0) continue;

                // Ensure product stock record exists
                var stock = await _context.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == line.ProductId);
                if (stock == null)
                {
                    stock = new ProductStock
                    {
                        Id = Guid.NewGuid(),
                        ProductId = line.ProductId,
                        QuantityAvailable = 0,
                        QuantityReserved = 0
                    };
                    _context.ProductStocks.Add(stock);
                }

                // Increase available stock and update PO line received quantity
                stock.QuantityAvailable += qtyToReceive;
                line.QuantityReceived += qtyToReceive;

                // Check product info
                var productInfo = await _context.Products
                    .Where(p => p.Id == line.ProductId)
                    .Select(p => new { p.IsManufactured, HasBOM = _context.BillOfMaterials.Any(b => b.ProductId == p.Id) })
                    .FirstOrDefaultAsync();

                // Reserve & fulfill for all pending sales order items if applicable
                if (productInfo != null && !productInfo.IsManufactured && !productInfo.HasBOM)
                {
                    var pendingSOItems = await _context.SalesOrderItems
                        .Include(s => s.SalesOrder)
                        .Where(s => s.ProductId == line.ProductId &&
                                    s.QuantityRequested > s.QuantityFulfilled)
                        .OrderBy(s => s.SalesOrder.OrderDate) // optional: FIFO
                        .ToListAsync();

                    foreach (var soItem in pendingSOItems)
                    {
                        // Reserve available stock
                        var saleorderItemStock = await _context.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == soItem.ProductId);
                        var reserveNeeded = soItem.QuantityRequested - soItem.QuantityReserved - soItem.QuantityFulfilled;
                        if (reserveNeeded > 0 && saleorderItemStock.QuantityAvailable > 0)
                        {
                            var reserveQty = Math.Min(saleorderItemStock.QuantityAvailable, reserveNeeded);
                            saleorderItemStock.QuantityAvailable -= reserveQty;
                            saleorderItemStock.QuantityReserved += reserveQty;
                            soItem.QuantityReserved += reserveQty;
                        }

                        // Fulfill from reserved stock
                        var fulfillNeeded = soItem.QuantityRequested - soItem.QuantityFulfilled;
                        var fulfillQty = Math.Min(soItem.QuantityReserved, fulfillNeeded);
                        if (fulfillQty > 0)
                        {
                            soItem.QuantityFulfilled += fulfillQty;
                            soItem.QuantityReserved -= fulfillQty;
                            stock.QuantityReserved -= fulfillQty;
                        }

                        affectedSalesOrderIds.Add(soItem.SalesOrderId);

                        if (stock.QuantityAvailable <= 0) break; // stop if stock depleted
                    }
                }

                // Add stock transaction
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = line.ProductId,
                    Quantity = qtyToReceive,
                    Type = "RECEIVE",
                    ReferenceId = purchaseOrder.Id,
                    Date = DateTime.UtcNow,
                    PerformedBy = "System",
                    Notes = $"Received from PO {purchaseOrder.OrderNumber}"
                });
            }

            // Update PO status
            purchaseOrder.Status = PurchaseOrderStatus.Received;
            purchaseOrder.ExpectedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Trigger MRP for affected sales orders
            foreach (var soId in affectedSalesOrderIds)
            {
                await _mrpService.RunMrpForSalesOrder(soId);
            }

            await transaction.CommitAsync();

            return Ok(new
            {
                purchaseOrder.Id,
                purchaseOrder.OrderNumber,
                Status = purchaseOrder.Status.ToString(),
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
