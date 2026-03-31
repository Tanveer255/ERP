namespace ERP.Entity.DTO;

public class CreateBOMDto
{
    public Guid ProductId { get; set; }       // Finished product
    public List<BOMComponentDto> Components { get; set; }
}

public class BOMComponentDto
{
    public Guid ComponentId { get; set; }     // Component product
    public decimal QuantityRequested { get; set; }     // QuantityRequested required
    public string Unit { get; set; }
}
