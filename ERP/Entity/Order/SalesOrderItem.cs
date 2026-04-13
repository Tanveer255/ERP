using ERP.Entity.Order;
using ERP.Entity.Product;

namespace ERP.Entity.Document;

public class SalesOrderItem
{
    public Guid Id { get; set; }

    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }

    public decimal QuantityRequested { get; set; }
    public decimal QuantityReserved { get; set; }

    //  NEW: actual fulfilled quantity (IMPORTANT)
    public decimal QuantityFulfilled { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    //  NEW: status per item
    public string Status { get; set; } // Pending / Partial / Completed

    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public ICollection<ProductionOrder> ProductionOrders { get; set; }

    // Navigation
    public SalesOrder SalesOrder { get; set; }
    public ProductEntity Product { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
