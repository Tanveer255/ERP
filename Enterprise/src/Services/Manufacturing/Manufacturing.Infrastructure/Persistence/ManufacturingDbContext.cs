using Manufacturing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Manufacturing.Infrastructure.Persistence;

public class ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options) : DbContext(options)
{
    public DbSet<BillOfMaterial> BillOfMaterials => Set<BillOfMaterial>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<MaterialConsumption> MaterialConsumptions => Set<MaterialConsumption>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<MrpRun> MrpRuns => Set<MrpRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillOfMaterial>(e =>
        {
            e.ToTable("bill_of_materials");
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.BomId);
        });
        modelBuilder.Entity<BomLine>(e => e.ToTable("bom_lines"));
        modelBuilder.Entity<ProductionOrder>(e => e.ToTable("production_orders"));
        modelBuilder.Entity<MaterialConsumption>(e => e.ToTable("material_consumptions"));
        modelBuilder.Entity<WorkCenter>(e => e.ToTable("work_centers"));
        modelBuilder.Entity<MrpRun>(e => e.ToTable("mrp_runs"));
    }
}
