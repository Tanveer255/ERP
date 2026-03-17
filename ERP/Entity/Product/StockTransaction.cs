using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Product;

public class StockTransaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    // The product/material being moved
    public Guid ProductId { get; set; }

    // Quantity of the movement (negative for issue, positive for receipt)
    public decimal Quantity { get; set; }

    // Type of transaction: ISSUE, RECEIPT, ADJUSTMENT, etc.
    [Required]
    [MaxLength(50)]
    public string Type { get; set; }

    // Reference to ProductionOrder, PurchaseOrder, etc.
    public Guid ReferenceId { get; set; }

    // Date and time of the transaction
    public DateTime Date { get; set; }

    // Optional: who performed the transaction
    [MaxLength(100)]
    public string PerformedBy { get; set; }

    // Optional: additional notes
    [MaxLength(500)]
    public string Notes { get; set; }
}
