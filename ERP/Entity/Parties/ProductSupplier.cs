using ERP.Entity.Product;

namespace ERP.Entity.Contact;

public class ProductSupplier
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Guid SupplierId { get; set; }

    public decimal Price { get; set; }
    public int LeadTimeInDays { get; set; }

    public bool IsPreferred { get; set; }

    public ProductEntity Product { get; set; }
    public Contact Supplier { get; set; }
}
