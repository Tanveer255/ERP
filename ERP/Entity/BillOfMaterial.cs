using ERP.Entity.Product;
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

    [JsonIgnore]
    [ForeignKey(nameof(ProductId))]
    public ProductEntity Product { get; set; }

    // Components for this product
    [JsonIgnore]
    public ICollection<BillOfMaterialItem> Items { get; set; } = new List<BillOfMaterialItem>();
}