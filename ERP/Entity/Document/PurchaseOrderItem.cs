namespace ERP.Entity.Document;

public class PurchaseOrderItem
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }

    public decimal RequestedQuantity { get; set; }
    public decimal QuantityReceived { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    //  ADD THIS
    public Guid? SalesOrderItemId { get; set; }
    public SalesOrderItem SalesOrderItem { get; set; }
    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; }
    public ProductEntity Product { get; set; }
}
