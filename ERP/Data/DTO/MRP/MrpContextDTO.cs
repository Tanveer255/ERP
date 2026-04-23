namespace ERP.Data.DTO.MRP;

public class MrpContextDTO
{
    public Guid ProductId { get; set; }
    public decimal QuantityRequested { get; set; }

    public Guid ReferenceId { get; set; } // SalesOrderId or ProductionOrderId
    public MrpSourceType SourceType { get; set; }
}

public enum MrpSourceType
{
    Sales,
    Production
}
