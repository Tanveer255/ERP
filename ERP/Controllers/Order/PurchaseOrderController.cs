using ERP.Data;
using ERP.Data.Request;
using ERP.Entity;
using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service;
using ERP.Service.Document;
using ERP.Service.Product;
using ERP.Service.Production;
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
    private readonly IProductService _productService;
    private readonly StockTransactionService _stockTransactionService;
    private readonly ProductStockService _productStockService;
    private readonly ProductionOrderService _productionOrderService;
    public PurchaseOrderController(
        ManufacturingDbContext context,
        PurchaseOrderService purchaseOrderService,
        SalesOrderService salesOrderService,
        MrpService mrpService,
        IProductService productService,
        StockTransactionService stockTransactionService,
        ProductStockService productStockService,
        ProductionOrderService productionOrderService
        )
    {
        _context = context;
        _purchaseOrderService = purchaseOrderService;
        _salesOrderService = salesOrderService;
        _mrpService = mrpService;
        _productService = productService;
        _stockTransactionService = stockTransactionService;
        _productStockService = productStockService;
        _productionOrderService = productionOrderService;
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
            //var saleOrderItems = purchaseOrder?.SalesOrders.SelectMany(so => so.Items).ToList();
            foreach (var line in purchaseOrder.Items)
            {
                var qtyToReceive = line.QuantityRequested - line.QuantityReceived;
                if (qtyToReceive <= 0) continue;

                var productStock = await _productStockService.GetStockStockByProductId(line.ProductId);
                var stock = productStock.Data;

                // =========================
                // STEP 1: RECEIVE STOCK
                // =========================
                stock.QuantityAvailable += qtyToReceive;
                line.QuantityReceived += qtyToReceive;

                await _stockTransactionService.AddReceiveTransactionAsync(new ReceiveTransactionRequest
                {
                    Type = StockTransactionType.RECEIVE,
                    ProductId = line.ProductId,
                    Quantity = qtyToReceive,
                    ReferenceId = purchaseOrder.Id,
                    ReferenceNumber = purchaseOrder.OrderNumber,
                    Note = "Received from PO",
                    PerformedBy = "System"
                });

                var isStockItem = await _productService.IsStockItemAsync(line.ProductId);

                if (isStockItem)
                {
                    var saleOrderData = await _salesOrderService.GetPendingSaleOrderItems(line.ProductId);

                    if (saleOrderData.IsSuccess)
                    {
                        foreach (var soItem in saleOrderData.Data)
                        {
                            var reserveNeeded = soItem.QuantityRequested - soItem.QuantityReserved - soItem.QuantityFulfilled;

                            if (reserveNeeded <= 0 || stock.QuantityAvailable <= 0)
                                continue;

                            // =========================
                            // STEP 2: RESERVE STOCK
                            // =========================
                            var reserveQty = Math.Min(stock.QuantityAvailable, reserveNeeded);

                            stock.QuantityAvailable -= reserveQty;
                            stock.QuantityReserved += reserveQty;
                            soItem.QuantityReserved += reserveQty;

                            await _stockTransactionService.AddReceiveTransactionAsync(new ReceiveTransactionRequest
                            {
                                Type = StockTransactionType.RESERVE,
                                ProductId = line.ProductId,
                                Quantity = reserveQty,
                                ReferenceId = soItem.SalesOrderId,
                                ReferenceNumber = purchaseOrder.OrderNumber,
                                Note = $"Reserved for SO {soItem.SalesOrderId}",
                                PerformedBy = "System"
                            });

                            // =========================
                            //  STEP 3: AUTO-FULFILL
                            // =========================
                            var fulfillQty = Math.Min(
                                soItem.QuantityReserved - soItem.QuantityFulfilled,
                                reserveQty
                            );

                            if (fulfillQty > 0)
                            {
                                soItem.QuantityFulfilled += fulfillQty;
                                soItem.QuantityReserved -= fulfillQty;
                                stock.QuantityReserved -= fulfillQty;

                                await _stockTransactionService.AddReceiveTransactionAsync(new ReceiveTransactionRequest
                                {
                                    Type = StockTransactionType.FULFILL,
                                    ProductId = line.ProductId,
                                    Quantity = fulfillQty,
                                    ReferenceId = soItem.SalesOrderId,
                                    ReferenceNumber = purchaseOrder.OrderNumber,
                                    Note = $"Auto-fulfilled SO {soItem.SalesOrderId}",
                                    PerformedBy = "System"
                                });
                            }

                            affectedSalesOrderIds.Add(soItem.SalesOrderId);

                            if (stock.QuantityAvailable <= 0)
                                break;
                        }
                    }
                    //var productionOrder = await _productionOrderService.LoadProductionOrderWithItems(line.ProductId);
                    //if (productionOrder.IsSuccess)
                    //{
                    //    if (productionOrder.Data != null)
                    //    {
                    //        foreach (var item in productionOrder.Data.BillOfMaterials.Items)
                    //        {
                    //            if (item.ComponentId == line.ProductId)
                    //            {
                    //               // await _mrpService.RunMrpForProductionShortage(productionOrder.Data.Id);
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}

                    // =========================
                    // UPDATE PO STATUS
                    // =========================
                    purchaseOrder.Status = PurchaseOrderStatus.Received;
                    purchaseOrder.ExpectedDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // =========================
                    // TRIGGER MRP
                    // =========================
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
