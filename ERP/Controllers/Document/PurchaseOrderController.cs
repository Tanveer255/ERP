using ERP.Data;
using ERP.Entity;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service;
using ERP.Service.Document;
using ERP.Service.Product;
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
    private readonly ProductService _productService;
    private readonly StockTransactionService _stockTransactionService;
    private readonly ProductStockService _productStockService;
    public PurchaseOrderController(
        ManufacturingDbContext context,
        PurchaseOrderService purchaseOrderService,
        SalesOrderService salesOrderService,
        MrpService mrpService,
        ProductService productService,
        StockTransactionService stockTransactionService,
        ProductStockService productStockService
        )
    {
        _context = context;
        _purchaseOrderService = purchaseOrderService;
        _salesOrderService = salesOrderService;
        _mrpService = mrpService;
        _productService = productService;
        _stockTransactionService = stockTransactionService;
        _productStockService = productStockService;
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
                var productStock = await _productStockService.GetStockStockByProductId(line.ProductId);
                //if (stock == null)
                //{
                //    stock = new ProductStock
                //    {
                //        Id = Guid.NewGuid(),
                //        ProductId = line.ProductId,
                //        QuantityAvailable = 0,
                //        QuantityReserved = 0
                //    };
                //    _context.ProductStocks.Add(stock);
                //}

                // Increase available stock and update PO line received quantity
                var stock = productStock.Data
                stock.QuantityAvailable += qtyToReceive;
                line.QuantityReceived += qtyToReceive;

                // Check product info
                var isStockItem = await _productService.IsStockItemAsync(line.ProductId);

                if (isStockItem)
                {
                    var result = await _salesOrderService.GetPendingSaleOrderItems(line.ProductId);
                    if (result.IsSuccess)
                    {
                        var pendingSOItems = result.Data;
                        foreach (var soItem in pendingSOItems)
                        {
                            // Reserve available stock
                            var reserveNeeded = soItem.QuantityRequested - soItem.QuantityReserved - soItem.QuantityFulfilled;
                            if (reserveNeeded > 0 && stock.QuantityAvailable > 0)
                            {
                                var reserveQty = Math.Min(stock.QuantityAvailable, reserveNeeded);
                                stock.QuantityAvailable -= reserveQty;
                                stock.QuantityReserved += reserveQty;
                                soItem.QuantityReserved += reserveQty;

                                // Optional: auto-fulfill
                                var fulfillQty = Math.Min(soItem.QuantityReserved - soItem.QuantityFulfilled, reserveQty);
                                if (fulfillQty > 0)
                                {
                                    soItem.QuantityFulfilled += fulfillQty;
                                    soItem.QuantityReserved -= fulfillQty;
                                    stock.QuantityReserved -= fulfillQty;

                                    _context.StockTransactions.Add(new StockTransaction
                                    {
                                        Id = Guid.NewGuid(),
                                        ProductId = line.ProductId,
                                        Quantity = fulfillQty,
                                        Type = "FULFILL",
                                        ReferenceId = soItem.SalesOrderId,
                                        Date = DateTime.UtcNow,
                                        PerformedBy = "System",
                                        Notes = $"Auto-fulfilled for Sales Order {soItem.SalesOrderId}"
                                    });
                                }
                                await _stockTransactionService.AddReceiveTransactionAsync("RECEIVE",
                                    line.ProductId, qtyToReceive, soItem.SalesOrderId,
                                    purchaseOrder.OrderNumber,
                                    $"Reserved from received PO {purchaseOrder.OrderNumber}");

                                //_context.StockTransactions.Add(new StockTransaction
                                //{
                                //    Id = Guid.NewGuid(),
                                //    ProductId = line.ProductId,
                                //    Quantity = reserveQty,
                                //    Type = "RESERVE",
                                //    ReferenceId = soItem.SalesOrderId,
                                //    Date = DateTime.UtcNow,
                                //    PerformedBy = "System",
                                //    Notes = $"Reserved from received PO {purchaseOrder.OrderNumber}"
                                //});
                            }

                            affectedSalesOrderIds.Add(soItem.SalesOrderId);

                            if (stock.QuantityAvailable <= 0) break; // stop if stock depleted
                        }
                    }
                }

                // Add stock transaction for receive
                await _stockTransactionService.AddReceiveTransactionAsync("RECEIVE", line.ProductId,
                    qtyToReceive, purchaseOrder.Id,
                    purchaseOrder.OrderNumber,
                    $"Received from PO ");
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
