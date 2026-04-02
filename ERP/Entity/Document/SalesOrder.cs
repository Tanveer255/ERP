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

    // Customer Info
    public string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }

    //  NEW: Track fulfillment summary
    public int TotalQuantity { get; set; }
    public int TotalFulfilledQuantity { get; set; }

    //  NEW: Optional link to Purchase Order (if 1 PO per SO)
    public Guid? PurchaseOrderId { get; set; }

    // Navigation
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}
