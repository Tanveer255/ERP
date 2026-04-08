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
        if (!po.IsSuccess) return NotFound(po.Message);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var salesOrderIds = new HashSet<Guid>();

            foreach (var item in po.Data.Items)
            {
                var remainingToReceive = item.QuantityRequested - item.QuantityReceived;
                if (remainingToReceive <= 0) continue;

                var newReceivedQty = remainingToReceive;

                // Get or create stock
                var stock = await _context.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == item.ProductId);
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

                stock.QuantityAvailable += newReceivedQty;
                item.QuantityReceived += newReceivedQty;

                // Get product info
                var product = await _context.Products
                    .Where(p => p.Id == item.ProductId)
                    .Select(p => new { p.IsManufactured, HasBOM = _context.BillOfMaterials.Any(b => b.ProductId == p.Id) })
                    .FirstOrDefaultAsync();

                // Only reserve if not manufactured and no BOM
                if (product != null && !product.IsManufactured && !product.HasBOM && item.SalesOrderItemId != Guid.Empty)
                {
                    var soItem = await _context.SalesOrderItems
                        .Include(s => s.SalesOrder)
                        .FirstOrDefaultAsync(s => s.Id == item.SalesOrderItemId);

                    if (soItem != null)
                    {
                        // Reserve stock
                        var reserveRemaining = soItem.QuantityRequested - soItem.QuantityReserved;
                        if (reserveRemaining > 0)
                        {
                            var reserveQty = Math.Min(stock.QuantityAvailable, reserveRemaining);
                            stock.QuantityAvailable -= reserveQty;
                            stock.QuantityReserved += reserveQty;
                            soItem.QuantityReserved += reserveQty;
                        }

                        // Fulfill from reserved stock
                        var fulfillRemaining = soItem.QuantityRequested - soItem.QuantityFulfilled;
                        if (fulfillRemaining > 0)
                        {
                            var fulfillQty = Math.Min(soItem.QuantityReserved - soItem.QuantityFulfilled, fulfillRemaining);
                            soItem.QuantityFulfilled += fulfillQty;
                            soItem.QuantityReserved -= fulfillQty;
                            stock.QuantityReserved -= fulfillQty;
                        }

                        salesOrderIds.Add(soItem.SalesOrderId);
                    }
                }

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

            po.Data.Status = PurchaseOrderStatus.Received;
            po.Data.ExpectedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Run MRP for affected sales orders
            foreach (var soId in salesOrderIds)
            {
                await _mrpService.RunMrpForSalesOrder(soId);
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
