using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class ProductionOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string OrderNumber { get; set; }

    public Guid ProductId { get; set; }      // Can be main or variant
    public ProductEntity Product { get; set; }

    public Guid BillOfMaterialId { get; set; }          // BOM always points to main product
    public BillOfMaterial BillOfMaterials { get; set; }

    public decimal PlannedQuantity { get; set; }

    public decimal ProducedQuantity { get; set; }

    public string Status { get; set; }       // Planned, InProgress, Completed

    public DateTime PlannedStartDate { get; set; }

    public DateTime PlannedFinishDate { get; set; }

    public DateTime? ActualStartDate { get; set; }

    public DateTime? ActualFinishDate { get; set; }

    public ICollection<ProductionOperation> Operations { get; set; }

    public ICollection<MaterialConsumption> Materials { get; set; }
}
