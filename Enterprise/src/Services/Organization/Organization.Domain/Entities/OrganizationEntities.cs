namespace Organization.Domain.Entities;

public class Company : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity, BuildingBlocks.Domain.IAggregateRoot
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ICollection<Plant> Plants { get; set; } = [];
}

public class Plant : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ICollection<Warehouse> Warehouses { get; set; } = [];
}

public class Warehouse : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid PlantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class Department : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CostCenter : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
