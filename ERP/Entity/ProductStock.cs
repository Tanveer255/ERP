using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class ProductStock
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public ProductEntity Product { get; set; }

    public decimal QuantityAvailable { get; set; } = 0.0m;  // Current stock available
    public decimal QuantityReserved { get; set; } = 0.0m;   // Reserved for production orders
    public decimal QuantityInProduction { get; set; } = 0.0m; // Currently being produced
    public decimal QuantityQuarantined { get; set; } = 0.0m; // Damaged/blocked stock
    public decimal QuantityRejected { get; set; } = 0.0m;   // Rejected/failed items
    public decimal QuantityExpired { get; set; } = 0.0m;   // Expired items

    public string Warehouse { get; set; } = "None";
    public string Zone { get; set; }
    public string Aisle { get; set; }
    public string Rack { get; set; }
    public string Shelf { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
