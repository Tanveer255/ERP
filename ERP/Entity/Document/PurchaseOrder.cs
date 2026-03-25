using ERP.Entity.Contact;
using ERP.Enum;

namespace ERP.Entity.Document;

public class PurchaseOrder
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; }

    public Guid SupplierId { get; set; }

    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDate { get; set; }

    public PurchaseOrderStatus Status { get; set; }

    // Financials
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public Contact.Contact Supplier { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; }
}
