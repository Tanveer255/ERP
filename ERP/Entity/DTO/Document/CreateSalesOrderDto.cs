namespace ERP.Entity.DTO.Document;

public class CreateSalesOrderDto
{
    public string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }

    public List<SalesOrderItemDto> Items { get; set; }
}
