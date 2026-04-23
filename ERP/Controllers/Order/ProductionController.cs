using ERP.Data;
using ERP.Data.DTO.Order;
using ERP.Entity;
using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service;
using ERP.Service.Document;
using ERP.Service.Product;
using ERP.Service.Production;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers.Order;

[Route("api/[controller]")]
[ApiController]
public class ProductionController : ControllerBase
{
    private readonly ManufacturingDbContext _context;
    private readonly ProductionOrderService _productionOrderService;
    private readonly PurchaseOrderService _purchaseOrderService;
    private readonly ProductionOperationService _productionOperationService;
    private readonly MrpService _mrpService;
    private readonly ProductStockService _productStockService;

    public ProductionController(ManufacturingDbContext context,
        ProductionOrderService productionOrderService,
        PurchaseOrderService purchaseOrderService,
        ProductionOperationService productionOperationService,
        MrpService mrpService,
        ProductStockService productStockService
        )
    {
        _context = context;
        _productionOrderService = productionOrderService;
        _purchaseOrderService = purchaseOrderService;
        _productionOperationService = productionOperationService;
        _mrpService = mrpService;
        _productStockService = productStockService;
    }
    #region Create Production Order
    [HttpPost("create-production-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderDto dto)
    {
        if (dto.QuantityRequested <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ProductId == dto.ProductId);

        var product = await _context.Products.FindAsync(bom.ProductId);
        if (product == null)
            return NotFound("Product not found.");


        if (bom == null || !bom.Items.Any())
            return BadRequest("No BOM defined.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _productionOrderService.CreateProductionOrderAsync(dto, bom);

            var supplierOrders = new Dictionary<Guid, PurchaseOrder>();

            _context.PurchaseOrders.AddRange(supplierOrders.Values);

            _productionOperationService.AddDefaultOperations(order.Id);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                order.Status
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
    #endregion

    [HttpPost("prepare-production")]
    public async Task<IActionResult> PrepareProduction(Guid orderId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null) return NotFound();

        if (order.Status != nameof(ProductionStatus.Planned))
            return BadRequest("Order must be in Planned state");

        // ✅ Run MRP (planning phase)
        await _mrpService.RunMrpForProductionShortage(orderId);

        // ✅ Check stock after MRP
        var hasStock = await _productStockService.CheckStockAvailability(orderId);

        if (!hasStock)
            return BadRequest("Stock still insufficient. Purchase orders created.");

        return Ok("Production prepared. Ready for material issue.");
    }

    #region Issue Materials
    [HttpPost("issue-material")]
    public async Task<IActionResult> IssueMaterials(Guid orderId)
    {
        var orderResult = await _productionOrderService.LoadProductionOrderWithItems(orderId);
        var order = orderResult.Data;

        if (order == null) return NotFound("Order not found");

        if (order.Status != nameof(ProductionStatus.Planned))
            return BadRequest("Order must be Planned");

        // ✅ NEW: Validate reserved stock BEFORE issuing
        foreach (var item in order.BillOfMaterials.Items)
        {
            var requiredQty = item.Quantity * order.PlannedQuantity;

            var stock = await _purchaseOrderService.GetStock(item.ComponentId);

            if (stock.QuantityReserved < requiredQty)
            {
                return BadRequest($"Insufficient reserved stock for component {item.ComponentId}. Run MRP/Reserve first.");
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in order.BillOfMaterials.Items)
            {
                var requiredQty = item.Quantity * order.PlannedQuantity;

                var success = await Helper.ExecuteWithRetryAsync(async () =>
                {
                    if (!await _purchaseOrderService.AdjustStock(
                            order.Id,
                            item.ComponentId,
                            requiredQty,
                            StockTransactionType.ISSUE))
                    {
                        throw new Exception("Stock issue failed");
                    }
                });

                if (!success)
                    return Conflict("Concurrency conflict");
            }

            order.Status = nameof(ProductionStatus.Ready);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Materials issued");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
    #endregion

    #region Create Production Order
    [HttpPost("start-production")]
    public async Task<IActionResult> StartProduction(Guid orderId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null) return NotFound();
        if (order.Status != nameof(ProductionStatus.Ready))
            return BadRequest("Not ready");

        // NEW: Run MRP here
        await _mrpService.RunMrpForProductionShortage(orderId);

        var firstOp = await _context.ProductionOperations
            .Where(x => x.OrderId == orderId)
            .OrderBy(x => x.SequenceNumber)
            .FirstOrDefaultAsync();

        var success = await Helper.ExecuteWithRetryAsync(async () =>
        {
            order.Status = nameof(ProductionStatus.InProgress);
            order.ActualStartDate = DateTime.UtcNow;

            if (firstOp != null)
                firstOp.Status = nameof(ProductionStatus.InProgress);

            await _context.SaveChangesAsync();
        });

        if (!success) return Conflict("Concurrency conflict");

        return Ok("Production started");
    }
    #endregion


    #region Advance Production
    [HttpPost("advance-production")]
    public async Task<IActionResult> AdvanceProduction(Guid orderId)
    {
        var nextOp = await _productionOrderService.AdvanceOperation(orderId);
        if (nextOp.Current == null) return BadRequest("No active operation");

        return Ok(new
        {
            Current = nextOp.Current?.OperationName,
            Next = nextOp.Next?.OperationName
        });
    }
    #endregion

    #region Complete Production
    [HttpPost("complete-production")]
    public async Task<IActionResult> CompleteProduction(Guid orderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.ProductionOrders.FindAsync(orderId);
            if (order == null) return NotFound();

            var bomItems = await _context.BillOfMaterialItems
                .Where(x => x.BillOfMaterialId == order.BillOfMaterialId)
                .ToListAsync();

            foreach (var item in bomItems)
            {
                var qty = item.Quantity * order.PlannedQuantity;
                await _purchaseOrderService.AdjustStock(order.Id, item.ComponentId, qty, StockTransactionType.CONSUME);
            }

            await _purchaseOrderService.AdjustStock(order.Id, order.ProductId, order.PlannedQuantity, StockTransactionType.RECEIPT);

            order.Status = nameof(ProductionStatus.Completed);
            order.ProducedQuantity = order.PlannedQuantity;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Production completed");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
    #endregion
}