using Manufacturing.Domain.Entities;

namespace Manufacturing.Domain.Repositories;

public interface IBomRepository : BuildingBlocks.Domain.Repositories.IRepository<BillOfMaterial>
{
    Task<IReadOnlyList<BillOfMaterial>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IProductionOrderRepository : BuildingBlocks.Domain.Repositories.IRepository<ProductionOrder>
{
    Task<IReadOnlyList<ProductionOrder>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
