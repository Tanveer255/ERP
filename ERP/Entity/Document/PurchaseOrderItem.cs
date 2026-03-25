namespace ERP.Entity.Document;

public class PurchaseOrderItem
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; }
    public ProductEntity Product { get; set; }
}
