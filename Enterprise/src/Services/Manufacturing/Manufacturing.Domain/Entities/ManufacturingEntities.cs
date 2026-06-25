namespace Manufacturing.Domain.Entities;

public enum TrackingType { None, Lot, Batch, Serial }

public class BillOfMaterial : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity, BuildingBlocks.Domain.IAggregateRoot
{
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public string Version { get; set; } = "1.0";
    public bool IsActive { get; set; } = true;
    public ICollection<BomLine> Lines { get; set; } = [];
}

public class BomLine : BuildingBlocks.Domain.Entity
{
    public Guid BomId { get; set; }
    public Guid ComponentProductId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "EA";
}

public class WorkCenter : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid PlantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CapacityHoursPerDay { get; set; }
}

public class ProductionOrder : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity, BuildingBlocks.Domain.IAggregateRoot
{
    public Guid TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid BomId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal ProducedQuantity { get; set; }
    public string Status { get; set; } = "Planned";
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedFinishDate { get; set; }
    public ICollection<MaterialConsumption> MaterialConsumptions { get; set; } = [];
}

public class MaterialConsumption : BuildingBlocks.Domain.Entity
{
    public Guid ProductionOrderId { get; set; }
    public Guid ComponentProductId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? LotId { get; set; }
}

public class MrpRun : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; set; }
    public DateTime RunDateUtc { get; set; }
    public string Status { get; set; } = "Running";
}
