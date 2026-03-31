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

    public int TotalItems { get; set; }
    public int ReservedItems { get; set; }
    public int PendingItems { get; set; }

    public List<PurchaseOrderSummaryDto> PurchaseOrders { get; set; } = new();
    public List<ProductionOrderSummaryDto> ProductionOrders { get; set; } = new();
}
public class PurchaseOrderSummaryDto
{
    public Guid SupplierId { get; set; }
    public int TotalItems { get; set; }
}
public class ProductionOrderSummaryDto
{
    public Guid ProductId { get; set; }
    public decimal PlannedQuantity { get; set; }
}