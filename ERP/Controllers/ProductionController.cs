using ERP.Data;
using ERP.Entity;
using ERP.Entity.Document;
using ERP.Entity.DTO;
using ERP.Entity.Product;
using ERP.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductionController : ControllerBase
{
    private readonly ManufacturingDbContext _context;

    public ProductionController(ManufacturingDbContext context)
    {
        _context = context;
    }

    #region 🔁 Retry Helper
    private async Task<bool> ExecuteWithRetryAsync(Func<Task> action, int maxRetry = 3)
    {
        int retry = maxRetry;

        while (retry > 0)
        {
            try
            {
                await action();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                retry--;
                if (retry == 0) return false;
            }
        }
        return false;
    }
    #endregion

    #region 🏭 Create Production Order
    [HttpPost("create-production-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderDto dto)
    {
        if (dto.ProductId == Guid.Empty)
            return BadRequest("ProductId is required.");

        if (dto.Quantity <= 0)
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
            var order = await CreateProductionOrderAsync(dto, bom);

            var supplierOrders = new Dictionary<Guid, PurchaseOrder>();

            await ReserveStockAndCreatePOs(order, bom, supplierOrders);

            _context.PurchaseOrders.AddRange(supplierOrders.Values);

            AddDefaultOperations(order.Id);

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

    #region 📦 Issue Materials
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

                var success = await ExecuteWithRetryAsync(async () =>
                {
                    var stock = await GetStock(item.ComponentId);

                    if (stock.QuantityReserved < requiredQty)
                        throw new Exception("Insufficient reserved stock");

                    stock.QuantityReserved -= requiredQty;
                    stock.QuantityInProduction += requiredQty;

                    AddStockTransaction(order.Id, item.ComponentId, requiredQty, StockTransactionType.ISSUE);

                    await _context.SaveChangesAsync();
                });

                if (!success)
                    return Conflict("Concurrency conflict");
            }

            order.Status = "Ready";

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

    #region ▶️ Start Production
    [HttpPost("start-production")]
    public async Task<IActionResult> StartProduction(Guid orderId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);

        if (order == null) return NotFound();
        if (order.Status != "Ready") return BadRequest("Not ready");

        var firstOp = await _context.ProductionOperations
            .Where(x => x.OrderId == orderId)
            .OrderBy(x => x.SequenceNumber)
            .FirstOrDefaultAsync();

        var success = await ExecuteWithRetryAsync(async () =>
        {
            order.Status = nameof(ProductionStatus.InProgress);
            order.ActualStartDate = DateTime.UtcNow;

            if (firstOp != null)
                firstOp.Status = nameof(ProductionStatus.InProgress);

            await _context.SaveChangesAsync();
        });

        if (!success)
            return Conflict("Concurrency conflict");

        return Ok("Production started");
    }
    #endregion

    #region 🔄 Advance Production
    [HttpPost("advance-production")]
    public async Task<IActionResult> AdvanceProduction(Guid orderId)
    {
        var currentOp = await _context.ProductionOperations
            .Where(x => x.OrderId == orderId && x.Status == nameof(ProductionStatus.InProgress))
            .OrderBy(x => x.SequenceNumber)
            .FirstOrDefaultAsync();

        if (currentOp == null) return BadRequest("No active operation");

        currentOp.Status = nameof(ProductionStatus.Completed);

        var nextOp = await _context.ProductionOperations
            .Where(x => x.OrderId == orderId && x.Status == nameof(ProductionStatus.Pending))
            .OrderBy(x => x.SequenceNumber)
            .FirstOrDefaultAsync();

        if (nextOp != null)
            nextOp.Status = nameof(ProductionStatus.InProgress);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Current = currentOp.OperationName,
            Next = nextOp?.OperationName
        });
    }
    #endregion

    #region ✅ Complete Production
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
                var stock = await GetStock(item.ComponentId);

                var qty = item.Quantity * order.PlannedQuantity;

                stock.QuantityInProduction -= qty;

                AddStockTransaction(order.Id, item.ComponentId, qty, StockTransactionType.CONSUME);
            }

            var finishedStock = await GetStock(order.ProductId);
            finishedStock.QuantityAvailable += order.PlannedQuantity;

            AddStockTransaction(order.Id, order.ProductId, order.PlannedQuantity, StockTransactionType.RECEIPT);

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

    #region 🧩 Private Methods

    private async Task<ProductionOrder> CreateProductionOrderAsync(CreateProductionOrderDto dto, BillOfMaterial bom)
    {
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PROD-{Guid.NewGuid().ToString()[..8]}",
            ProductId = dto.ProductId,
            BillOfMaterialId = bom.Id,
            PlannedQuantity = dto.Quantity,
            Status = nameof(ProductionStatus.Planned),
            PlannedStartDate = dto.StartDate,
            PlannedFinishDate = dto.FinishDate
        };

        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    private async Task ReserveStockAndCreatePOs(
        ProductionOrder order,
        BillOfMaterial bom,
        Dictionary<Guid, PurchaseOrder> supplierOrders)
    {
        foreach (var item in bom.Items)
        {
            var stock = await GetStock(item.ComponentId);

            var requiredQty = item.Quantity * order.PlannedQuantity;

            var reserved = Math.Min(stock.QuantityAvailable, requiredQty);

            stock.QuantityAvailable -= reserved;
            stock.QuantityReserved += reserved;

            var shortage = requiredQty - reserved;

            if (shortage > 0)
                await HandleSupplier(item, shortage, supplierOrders);

            AddStockTransaction(order.Id, item.ComponentId, reserved, StockTransactionType.RESERVE);
        }
    }

    private async Task HandleSupplier(BillOfMaterialItem item, decimal shortage,
        Dictionary<Guid, PurchaseOrder> supplierOrders)
    {
        var supplier = await _context.ProductSuppliers
            .Where(x => x.ProductId == item.ComponentId)
            .OrderByDescending(x => x.IsPreferred)
            .ThenBy(x => x.Price)
            .FirstOrDefaultAsync();

        if (supplier == null)
            throw new Exception("No supplier found");

        if (!supplierOrders.ContainsKey(supplier.SupplierId))
        {
            supplierOrders[supplier.SupplierId] = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"PO-{Guid.NewGuid().ToString()[..8]}",
                SupplierId = supplier.SupplierId,
                OrderDate = DateTime.UtcNow,
                ExpectedDate = DateTime.UtcNow.AddDays(supplier.LeadTimeInDays),
                Status = PurchaseOrderStatus.Draft,
                Items = new List<PurchaseOrderItem>()
            };
        }

        supplierOrders[supplier.SupplierId].Items.Add(new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = item.ComponentId,
            Quantity = shortage,
            UnitPrice = supplier.Price
        });
    }

    private async Task<ProductStock> GetStock(Guid productId)
    {
        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (stock == null)
            throw new Exception($"Stock not found for {productId}");

        return stock;
    }

    private void AddStockTransaction(Guid orderId, Guid productId, decimal qty, StockTransactionType type)
    {
        _context.StockTransactions.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = qty,
            Type = nameof(type),
            ReferenceId = orderId,
            Date = DateTime.UtcNow,
            PerformedBy = "SYSTEM"
        });
    }

    private void AddDefaultOperations(Guid orderId)
    {
        _context.ProductionOperations.AddRange(new List<ProductionOperation>
        {
            new() { Id = Guid.NewGuid(), OrderId = orderId, OperationName = "Cutting", SequenceNumber = 1, Status = "Pending" },
            new() { Id = Guid.NewGuid(), OrderId = orderId, OperationName = "Assembly", SequenceNumber = 2, Status = "Pending" },
            new() { Id = Guid.NewGuid(), OrderId = orderId, OperationName = "Packaging", SequenceNumber = 3, Status = "Pending" }
        });
    }

    #endregion
}