namespace ERP.Entity.Document;

public class GoodsReceipt
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public DateTime ReceiptDate { get; set; }

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; }
    public ProductEntity Product { get; set; }
}
