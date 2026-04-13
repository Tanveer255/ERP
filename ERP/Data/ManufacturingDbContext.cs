using ERP.Entity;
using ERP.Entity.Contact;
using ERP.Entity.Document;
using ERP.Entity.Order;
using ERP.Entity.Product;
using ERP.Entity.Settings;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;

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
    public DbSet<ProductStock> ProductStocks { get; set; }
    public DbSet<Price> Prices { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }
    public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }
    public DbSet<Contact> Contact { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<ProductSupplier> ProductSuppliers { get; set; }
    public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
    public DbSet<MrpPlan> MrpPlans { get; set; }
    public DbSet<InventorySettings> InventorySettings { get; set; }
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
        var unitOfmeasure = LoadUnitOfMeasureFromJson("Data/Seed/unitOfmeasure.json");
        modelBuilder.Entity<UnitOfMeasure>().HasData(unitOfmeasure);
        // Finished product relationship
        modelBuilder.Entity<BillOfMaterial>()
            .HasOne(b => b.Product)           // navigation property
            .WithMany(p => p.BOMs)            // main product has many BOMs
            .HasForeignKey(b => b.ProductId)  // foreign key
            .OnDelete(DeleteBehavior.Restrict);

        // Optional: Configure variant relationship in Product
        modelBuilder.Entity<ProductEntity>()
            .HasMany(p => p.Variants)
            .WithOne(v => v.MainProduct)
            .HasForeignKey(v => v.MainProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductStock>()
        .Property(p => p.RowVersion)
        .IsRowVersion();

        modelBuilder.Entity<ProductionOrder>()
            .Property(p => p.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<Price>()
        .HasOne(p => p.Product)
        .WithMany(p => p.Prices)
        .HasForeignKey(p => p.ProductId)
        .OnDelete(DeleteBehavior.Cascade);

    }
    private List<UnitOfMeasure> LoadUnitOfMeasureFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<UnitOfMeasure>>(json) ?? new();
    }
}