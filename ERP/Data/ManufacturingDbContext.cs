using Microsoft.EntityFrameworkCore;
using ERP.Entity;
using System.Collections.Generic;
using ERP.Entity.Product;

namespace ERP.Data;

public class ManufacturingDbContext : DbContext
{
    public ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<BillOfMaterial> BillOfMaterials { get; set; }
    public DbSet<BillOfMaterialItem> BillOfMaterialItems { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProductionOperation> ProductionOperations { get; set; }
    public DbSet<MaterialConsumption> MaterialConsumptions { get; set; }
    public DbSet<FinishedGoodsReceipt> FinishedGoodsReceipts { get; set; }
    public DbSet<ProductStock> productStocks { get; set; }
    public DbSet<ERP.Entity.Product.Price> prices { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductStock>(entity =>
        {
            entity.Property(p => p.QuantityAvailable).HasDefaultValue(0.0m);
            entity.Property(p => p.QuantityReserved).HasDefaultValue(0.0m);
            entity.Property(p => p.QuantityInProduction).HasDefaultValue(0.0m);
            entity.Property(p => p.QuantityQuarantined).HasDefaultValue(0.0m);
            entity.Property(p => p.QuantityRejected).HasDefaultValue(0.0m);
            entity.Property(p => p.QuantityExpired).HasDefaultValue(0.0m);

            entity.Property(p => p.Warehouse).HasDefaultValue("None");
            entity.Property(p => p.Zone).HasDefaultValue("None");
            entity.Property(p => p.Aisle).HasDefaultValue("None");
            entity.Property(p => p.Rack).HasDefaultValue("None");
            entity.Property(p => p.Shelf).HasDefaultValue("None");
        });

        // Finished product relationship
        modelBuilder.Entity<BillOfMaterial>()
            .HasOne(b => b.Product)           // navigation property
            .WithMany(p => p.BOMs)            // main product has many BOMs
            .HasForeignKey(b => b.ProductId)  // foreign key
            .OnDelete(DeleteBehavior.Restrict);

        // Component relationship
        modelBuilder.Entity<BillOfMaterial>()
            .HasOne(b => b.Component)         // navigation property
            .WithMany()                        // component may be in multiple BOMs
            .HasForeignKey(b => b.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional: Configure variant relationship in Product
        modelBuilder.Entity<ProductEntity>()
            .HasMany(p => p.Variants)
            .WithOne(v => v.MainProduct)
            .HasForeignKey(v => v.MainProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
