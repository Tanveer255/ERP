namespace ERP.Enum;

public enum PurchaseOrderStatus
{
    Draft,        // Created automatically (MRP)
    Sent,         // Sent to supplier
    Confirmed,    // Supplier accepted
    PartiallyReceived,
    Received,
    Cancelled
}
