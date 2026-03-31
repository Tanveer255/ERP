namespace ERP.Entity;

public class MrpPlan
{
    public Guid Id { get; set; }

    // Links to Sales Order or Production/Purchase Order
    public Guid SalesOrderId { get; set; }

    // Optional: Production or Purchase Order to be generated
    public Guid? ProductionOrderId { get; set; }
    public Guid? PurchaseOrderId { get; set; }

    // Material / Product
    public Guid ProductId { get; set; }

    public decimal RequiredQuantity { get; set; }    // Quantity needed
    public decimal AvailableQuantity { get; set; }   // Stock + scheduled receipts
    public decimal PlannedQuantity { get; set; }     // Quantity to produce or purchase

    // Timing
    public DateTime RequiredDate { get; set; }       // When material is needed
    public DateTime? PlannedDate { get; set; }       // Suggested production/purchase date

    public bool IsProcessed { get; set; } = false;   // Flag if order generated
    public string Notes { get; set; }
}