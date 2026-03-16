using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }

    public bool IsManufactured { get; set; }  // true for any product we manufacture

    // Optional: if this is a variant, it links to the main product
    public Guid? MainProductId { get; set; }
    public Product MainProduct { get; set; }

    // All variants of this product (empty if this is a variant itself)
    public ICollection<Product> Variants { get; set; } = new List<Product>();

    public ICollection<BillOfMaterial> BOMs { get; set; } = new List<BillOfMaterial>();
}