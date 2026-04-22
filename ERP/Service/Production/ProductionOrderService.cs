using ERP.Data;
using ERP.Data.DTO;
using ERP.Data.DTO.Order;
using ERP.Entity.BOM;
using ERP.Entity.Document;
using ERP.Entity.Order;
using ERP.Enum;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Production;

public class ProductionOrderService
{
    private readonly ManufacturingDbContext _context;
    public ProductionOrderService( ManufacturingDbContext manufacturingDbContext)
    {
            _context = manufacturingDbContext;
    }
    public async Task<ResultDTO<ProductionOrder>> LoadProductionOrderWithItems(Guid orderId)
    {
        var order = await _context.ProductionOrders
           .Include(o => o.BillOfMaterials)
           .ThenInclude(b => b.Items)
           .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return ResultDTO<ProductionOrder>.Failure("Production order not found.");
        return ResultDTO<ProductionOrder>.Success(order);
    }
    public async Task<ProductionOrder> CreateProductionOrderAsync(CreateProductionOrderDto dto, BillOfMaterial bom)
    {
        var shortId = Guid.NewGuid().ToString("N")[..8];
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PROD-{shortId}",
            ProductId = dto.ProductId,
            BillOfMaterialId = bom.Id,
            PlannedQuantity = dto.QuantityRequested,
            Status = nameof(ProductionStatus.Planned),
            PlannedStartDate = dto.StartDate,
            PlannedFinishDate = dto.FinishDate
        };

        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }
    public async Task<(ProductionOperation? Current, ProductionOperation? Next)> AdvanceOperation(Guid orderId)
    {
        var currentOp = await _context.ProductionOperations
            .Where(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.InProgress))
            .OrderBy(o => o.SequenceNumber)
            .FirstOrDefaultAsync();

        if (currentOp == null) return (null, null);

        currentOp.Status = nameof(ProductionStatus.Completed);

        var nextOp = await _context.ProductionOperations
            .Where(o => o.OrderId == orderId && o.Status == nameof(ProductionStatus.Pending))
            .OrderBy(o => o.SequenceNumber)
            .FirstOrDefaultAsync();

        if (nextOp != null) nextOp.Status = nameof(ProductionStatus.InProgress);

        await _context.SaveChangesAsync();
        return (currentOp, nextOp);
    }
}
