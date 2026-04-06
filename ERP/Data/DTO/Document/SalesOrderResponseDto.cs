using ERP.Enum;

namespace ERP.Data.DTO.Document;

public class SalesOrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public SalesOrderStatus Status { get; set; }
    public List<SalesOrderItemResponseDto> Items { get; set; }
}

public class SalesOrderItemResponseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }   // optional but useful
    public decimal QuantityRequested { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityShort { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}