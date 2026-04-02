namespace ERP.Enum;

public enum SalesOrderStatus
{
    Draft = 0,
    //Order created but not yet confirmed by user/customer
    // Can still be edited or deleted

    Confirmed = 1,
    // Order confirmed by customer/user
    // Demand is now considered "real"

    Pending = 2,
    // Waiting for processing (approval / stock allocation / system actions)

    Processing = 3,
    // System is fulfilling the order (stock reservation / production / purchase)

    Partial = 4,
    // Some items reserved from stock, but not fully available

    FullyReserved = 5,
    // All items successfully reserved from stock

    Completed = 6,
    // Order fully fulfilled (delivered / closed)

    Cancelled = 7
    // Order cancelled
}