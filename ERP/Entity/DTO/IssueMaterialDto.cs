namespace ERP.Entity.DTO;

public class IssueMaterialDto
{
    public Guid OrderId { get; set; }
    public Guid MaterialId { get; set; }
    public decimal QuantityPlanned { get; set; }
    public decimal QuantityConsumed { get; set; }
}