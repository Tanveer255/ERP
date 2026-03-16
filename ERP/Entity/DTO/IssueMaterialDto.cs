namespace ERP.Entity.DTO;

public class IssueMaterialDto
{
    public Guid OrderId { get; set; }
    public Guid MaterialId { get; set; }
    public decimal PlannedQty { get; set; }
    public decimal ConsumedQty { get; set; }
}