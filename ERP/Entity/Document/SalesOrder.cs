using ERP.Enum;

namespace ERP.Entity.Document;

public class SalesOrder
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; }

    public DateTime OrderDate { get; set; }

    public SalesOrderStatus Status { get; set; }
    public ReservationStatus ReservationStatus { get; set; }

    public decimal TotalAmount { get; set; }

    // Customer Info (simple for now)
    public string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }

    // Navigation
    public ICollection<SalesOrderItem> Items { get; set; }
}
