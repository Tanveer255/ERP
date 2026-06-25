using BuildingBlocks.Infrastructure.Persistence;
using Manufacturing.Domain.Entities;
using Manufacturing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Manufacturing.Infrastructure.Persistence.Repositories;

public class BomRepository(ManufacturingDbContext context) : EfRepository<BillOfMaterial>(context), IBomRepository
{
    private ManufacturingDbContext Db => (ManufacturingDbContext)Context;

    public async Task<IReadOnlyList<BillOfMaterial>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Db.BillOfMaterials.Include(x => x.Lines).Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken);
}

public class ProductionOrderRepository(ManufacturingDbContext context) : EfRepository<ProductionOrder>(context), IProductionOrderRepository
{
    private ManufacturingDbContext Db => (ManufacturingDbContext)Context;

    public async Task<IReadOnlyList<ProductionOrder>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Db.ProductionOrders.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.PlannedStartDate).ToListAsync(cancellationToken);
}

public class ManufacturingUnitOfWork(ManufacturingDbContext context) : UnitOfWork(context), BuildingBlocks.Domain.Repositories.IUnitOfWork;
