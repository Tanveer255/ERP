namespace Product.Domain.Entities;

public class Product : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity, BuildingBlocks.Domain.IAggregateRoot
{
    public Guid TenantId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Unit { get; set; } = "EA";
    public decimal UnitCost { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsManufactured { get; set; }
    public TrackingType Tracking { get; set; } = TrackingType.None;
}

public enum TrackingType { None, Lot, Batch, Serial }

public class ProductVariant : BuildingBlocks.Domain.Entity
{
    public Guid ProductId { get; set; }
    public string VariantCode { get; set; } = string.Empty;
    public string? AttributesJson { get; set; }
}
