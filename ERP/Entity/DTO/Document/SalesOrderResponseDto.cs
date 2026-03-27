using ERP.Enum;

namespace ERP.Entity.DTO.Document;

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
    public decimal RequestedQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal ShortQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}