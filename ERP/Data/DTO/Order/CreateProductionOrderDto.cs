namespace ERP.Data.DTO.Order;

public class CreateProductionOrderDto
{
    public Guid ProductId { get; set; }
    public decimal QuantityRequested { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
}
