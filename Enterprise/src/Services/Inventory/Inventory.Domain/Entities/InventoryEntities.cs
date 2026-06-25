namespace Inventory.Domain.Entities;

public class StockBalance : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity, BuildingBlocks.Domain.IAggregateRoot
{
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    public string? LotNumber { get; set; }
    public string? SerialNumber { get; set; }
}

public class StockTransaction : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
}
