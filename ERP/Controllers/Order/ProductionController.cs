using ERP.Data;
using ERP.Data.DTO.Order;
using ERP.Entity;
using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service;
using ERP.Service.Document;
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

    public ProductionController(ManufacturingDbContext context,
        ProductionOrderService productionOrderService,
        PurchaseOrderService purchaseOrderService,
        ProductionOperationService productionOperationService,
        MrpService mrpService
        )
    {
        _context = context;
        _productionOrderService = productionOrderService;
        _purchaseOrderService = purchaseOrderService;
        _productionOperationService = productionOperationService;
        _mrpService = mrpService;
    }

    #region Create Production Order
    [HttpPost("create-production-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderDto dto)
    {
        if (dto.ProductId == Guid.Empty)
            return BadRequest("ProductId is required.");

        if (dto.QuantityRequested <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return NotFound("Product not found.");

        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ProductId == dto.ProductId);

        if (bom == null || !bom.Items.Any())
            return BadRequest("No BOM defined.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _productionOrderService.CreateProductionOrderAsync(dto, bom);

            var supplierOrders = new Dictionary<Guid, PurchaseOrder>();

            await _mrpService.RunMrpForSalesOrder(order.Id);

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

    #region Issue Materials
    [HttpPost("issue-material")]
    public async Task<IActionResult> IssueMaterials(Guid orderId)
    {
        var order = await _context.ProductionOrders
            .Include(o => o.BillOfMaterials)
            .ThenInclude(b => b.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return NotFound("Order not found");
        if (order.Status != nameof(ProductionStatus.Planned))
            return BadRequest("Order must be Planned");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in order.BillOfMaterials.Items)
            {
                var requiredQty = item.Quantity * order.PlannedQuantity;

                var success = await Helper.ExecuteWithRetryAsync(async () =>
                {
                    if (!await _purchaseOrderService.AdjustStock(order.Id, item.ComponentId, requiredQty, StockTransactionType.ISSUE))
                        throw new Exception("Insufficient reserved stock");
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

    #region Start Production
    [HttpPost("start-production")]
    public async Task<IActionResult> StartProduction(Guid orderId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null) return NotFound();
        if (order.Status != nameof(ProductionStatus.Ready)) return BadRequest("Not ready");

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