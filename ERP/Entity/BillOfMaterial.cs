using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Entity;

public class BillOfMaterial
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    // Finished product (parent)
    [Required]
    public Guid ProductId { get; set; }

    [JsonIgnore] // prevents circular reference
    [ForeignKey(nameof(ProductId))]
    public ProductEntity Product { get; set; }

    // Component product (child)
    [Required]
    public Guid ComponentId { get; set; }

    [ForeignKey(nameof(ComponentId))]
    public ProductEntity Component { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
}