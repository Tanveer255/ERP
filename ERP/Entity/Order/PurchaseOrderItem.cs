using ERP.Entity.Order;
using ERP.Entity.Product;
using System.Text.Json.Serialization;

namespace ERP.Entity.Document;

public class PurchaseOrderItem
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }

    public decimal QuantityRequested { get; set; }
    public decimal QuantityReceived { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    //  ADD THIS
    public Guid? SalesOrderItemId { get; set; }
    [JsonIgnore]
    public SalesOrderItem SalesOrderItem { get; set; }
    // Navigation
    [JsonIgnore]
    public PurchaseOrder PurchaseOrder { get; set; }
    public ProductEntity Product { get; set; }
    public Guid? ProductionOrderId { get; set; }
    public ProductionOrder ProductionOrder { get; set; }
}
