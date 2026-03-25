using ERP.Entity.Document;

namespace ERP.Entity.Contact;

public class Contact
{
    public Guid Id { get; set; }

    // Basic Info
    public string Name { get; set; }
    public string? Code { get; set; } // e.g. SUP-001

    // Contact Info
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    // Business Info
    public string? ContactPerson { get; set; }
    public string? TaxNumber { get; set; } // NTN / VAT

    // Procurement Settings
    public int DefaultLeadTimeInDays { get; set; } = 0;
    public string? Currency { get; set; } // PKR, USD, etc.

    // Status
    public bool IsActive { get; set; } = true;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public ICollection<ProductSupplier> ProductSuppliers { get; set; }
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
}
