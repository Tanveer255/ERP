using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class ProductionOperation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string OperationName { get; set; }

    public int SequenceNumber { get; set; }

    public string Status { get; set; }
    public DateTime CompletedDate { get; set; }

    public ProductionOrder Order { get; set; }
}