namespace ERP.Data.DTO;

public class MrpPlanDto
{
    public Guid ProductId { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal PlannedQuantity { get; set; }
    public DateTime RequiredDate { get; set; }
    public string Notes { get; set; }
}
