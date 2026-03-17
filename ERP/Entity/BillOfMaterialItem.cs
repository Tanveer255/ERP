using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class BillOfMaterialItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid BOMId { get; set; }

    // Component product
    public Guid ComponentId { get; set; }
    public ProductEntity Component { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; }

    public BillOfMaterial BOM { get; set; }
}
