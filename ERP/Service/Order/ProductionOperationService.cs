using ERP.Data;
using ERP.Entity.Order;
using ERP.Enum;

namespace ERP.Service.Document;

public class ProductionOperationService
{
    private readonly ManufacturingDbContext _context;
    public ProductionOperationService(ManufacturingDbContext manufacturingDbContext)
    {
       _context = manufacturingDbContext;
    }
    public void AddDefaultOperations(Guid orderId)
    {
        _context.ProductionOperations.AddRange(new List<ProductionOperation>
        {
            new() { Id = Guid.NewGuid(), OrderId = orderId, OperationName = "Cutting", SequenceNumber = 1, Status = nameof(ProductionStatus.Pending) },
            new() { Id = Guid.NewGuid(), OrderId = orderId, OperationName = "Assembly", SequenceNumber = 2, Status = nameof(ProductionStatus.Pending) },
            new() { Id = Guid.NewGuid(), OrderId = orderId, OperationName = "Packaging", SequenceNumber = 3, Status = nameof(ProductionStatus.Pending) }
        });
    }
}
