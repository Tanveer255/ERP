namespace ERP.Entity.Document;

public class SalesOrderItem
{
    public Guid Id { get; set; }

    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }

    public decimal RequestedQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public ICollection<ProductionOrder> ProductionOrders { get; set; }
    // Navigation
    public SalesOrder SalesOrder { get; set; }
    public ProductEntity Product { get; set; }
}
