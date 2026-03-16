namespace ERP.Entity.DTO;

public class CreateProductionOrderDto
{
    public Guid ProductId { get; set; }
    public Guid BillOfMaterialId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
}
