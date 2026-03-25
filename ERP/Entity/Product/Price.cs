namespace ERP.Entity.Product;

public class Price
{
    public int Id { get; set; }

    // Product relation (Foreign Key)
    public Guid ProductId { get; set; }

    public ProductEntity Product { get; set; } // navigation property

    // Cost & Selling
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }

    // Discount
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }

    // Tax
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }

    // Final Price after discount + tax
    public decimal FinalPrice { get; set; }

    // Currency (PKR, USD etc.)
    public string Currency { get; set; } = "PKR";

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
