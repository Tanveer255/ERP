using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity;
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
    public async Task<ProductionOrder> CreateProductionOrderAsync(CreateProductionOrderDto dto, BillOfMaterial bom)
    {
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PROD-{Guid.NewGuid():N8}",
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
