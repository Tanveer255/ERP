using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class BillOfMaterial
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    // Finished product
    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    // Component product
    public Guid ComponentId { get; set; }
    public Product Component { get; set; }

    public decimal Quantity { get; set; }
}