using ERP.Entity.Document;
using System.ComponentModel.DataAnnotations;

namespace ERP.Entity.DTO.Document;

public class CreateSalesOrderDto
{
    [Required(ErrorMessage = "Customer name is required")]
    public string CustomerName { get; set; }

    public string? CustomerEmail { get; set; }  // Optional

    [Required(ErrorMessage = "At least one item is required")]
    [MinLength(1, ErrorMessage = "At least one item must be provided")]
    public List<SalesOrderItemDto> Items { get; set; } = new List<SalesOrderItemDto>();
}

public class CreateSalesOrderResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsStockAvailable { get; set; }
    public string Message { get; set; }

    public int TotalItems { get; set; }
    public int ReservedItems { get; set; }
    public int PendingItems { get; set; }

    public List<PurchaseOrderSummaryDto> PurchaseOrders { get; set; } = new();
    public List<ProductionOrderSummaryDto> ProductionOrders { get; set; } = new();
}
public class PurchaseOrderSummaryDto
{
    public Guid PurchaseOrderId { get; set; }      // Unique PO ID
    public string OrderNumber { get; set; }        // PO Number
    public Guid SupplierId { get; set; }           // Supplier reference
    public int TotalItems { get; set; }            // Total items in PO
    public string Status { get; set; }             // Status (e.g., Pending, Received)
    public decimal PendingItems { get; set; } // Must be decimal
}
public class ProductionOrderSummaryDto
{
    public Guid ProductionOrderId { get; set; }    // Unique MO ID
    public string OrderNumber { get; set; }        // MO Number
    public Guid ProductId { get; set; }            // Product being produced
    public decimal PlannedQuantity { get; set; }   // Planned quantity to produce
    public decimal ProducedQuantity { get; set; }  // Already produced quantity
    public string Status { get; set; }             // Current status (Planned, InProgress, Completed)
    public DateTime PlannedStartDate { get; set; } // Scheduled start
    public DateTime PlannedFinishDate { get; set; }// Scheduled finish
    public DateTime? ActualStartDate { get; set; } // Actual start
    public DateTime? ActualFinishDate { get; set; }// Actual finish
}