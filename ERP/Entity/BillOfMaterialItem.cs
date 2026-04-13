using ERP.Entity.Product;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Entity;

public class BillOfMaterialItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid BillOfMaterialId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(BillOfMaterialId))]
    public BillOfMaterial BillOfMaterial { get; set; }

    [Required]
    public Guid ComponentId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(ComponentId))]
    public ProductEntity Component { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; }
}
