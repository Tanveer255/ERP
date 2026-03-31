namespace ERP.Enum;

public enum PurchaseOrderStatus
{
    Draft = 0,
    // System-generated or manually created PO (not yet sent to supplier)

    Pending = 1,
    // Waiting for internal approval or processing before sending

    Sent = 2,
    //Purchase order has been sent to the supplier

    Confirmed = 3,
    // Supplier has accepted/confirmed the order

    PartiallyReceived = 4,
    // Some items have been received, but not the full order

    Received = 5,
    // All items have been fully received

    Cancelled = 6
    // Purchase order has been cancelled (no further processing)
}
