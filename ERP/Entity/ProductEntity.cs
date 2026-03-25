using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class ProductEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }

    public bool IsManufactured { get; set; }  // true for any product we manufacture
    public decimal UnitCost { get; set; }

    // Optional: if this is a variant, it links to the main product
    public Guid? MainProductId { get; set; }
    public ProductEntity MainProduct { get; set; }
    public bool IsPurchasable { get; set; }

    // All variants of this product (empty if this is a variant itself)
    public ICollection<ProductEntity> Variants { get; set; } = new List<ProductEntity>();

    public ICollection<BillOfMaterial> BOMs { get; set; } = new List<BillOfMaterial>();
   
}