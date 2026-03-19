using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Entity;

public class ProductionOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    // Unique order number for reference
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; }

    // The product being manufactured (main or variant)
    [Required]
    public Guid ProductId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(ProductId))]
    public ProductEntity Product { get; set; }

    // Reference to the BOM used for this production
    [Required]
    public Guid BillOfMaterialId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(BillOfMaterialId))]
    public BillOfMaterial BillOfMaterials { get; set; }

    // Quantities
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal PlannedQuantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ProducedQuantity { get; set; }

    // Workflow status: Planned, InProgress, Completed
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Planned";

    // Scheduled dates
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedFinishDate { get; set; }

    // Actual dates
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualFinishDate { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; }

    // Production operations (e.g., routing steps)
    [JsonIgnore]
    public ICollection<ProductionOperation> Operations { get; set; } = new List<ProductionOperation>();

    // Materials consumed against this order
    [JsonIgnore]
    public ICollection<MaterialConsumption> Materials { get; set; } = new List<MaterialConsumption>();
}
