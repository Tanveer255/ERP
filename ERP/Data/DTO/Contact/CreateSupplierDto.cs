namespace ERP.Data.DTO.Contact;

public class CreateSupplierDto
{
    public string Name { get; set; }
    public string? Code { get; set; }      // e.g., SUP-001
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ContactPerson { get; set; }
    public string? TaxNumber { get; set; }
    public int DefaultLeadTimeInDays { get; set; } = 3;
    public string? Currency { get; set; } = "PKR";
    // Products the supplier can supply
    public List<SupplierProductDto> Products { get; set; } = new();
}
// Products the supplier can supply
public class SupplierProductDto
{
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public int LeadTimeInDays { get; set; } = 3;
    public bool IsPreferred { get; set; } = true;
}