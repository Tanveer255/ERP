using Microsoft.EntityFrameworkCore;
using Product.Domain.Entities;

namespace Product.Infrastructure.Persistence;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product.Domain.Entities.Product> Products => Set<Product.Domain.Entities.Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product.Domain.Entities.Product>(e =>
        {
            e.ToTable("products");
            e.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
        });
        modelBuilder.Entity<ProductVariant>(e => e.ToTable("product_variants"));
    }
}
