using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Entity;

public class MaterialConsumption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    public Guid MaterialId { get; set; }

    public decimal PlannedQuantity { get; set; }

    public decimal ConsumedQuantity { get; set; }

    public DateTime ConsumptionDate { get; set; }

    [JsonIgnore]
    public ProductionOrder Order { get; set; }
}
