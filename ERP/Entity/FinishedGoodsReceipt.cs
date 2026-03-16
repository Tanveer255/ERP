using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

public class FinishedGoodsReceipt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public DateTime ReceiptDate { get; set; }

    public ProductionOrder Order { get; set; }
}
