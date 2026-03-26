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
    public List<SalesOrderItemDto> Items { get; set; }
}

//public class SalesOrderItemDto
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public int Quantity { get; set; }
//    public int QuantityReserved { get; set; }
//    public decimal UnitPrice { get; set; }
//    public decimal TotalPrice { get; set; }
//}
