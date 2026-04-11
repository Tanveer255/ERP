using ERP.Enum;

namespace ERP.Data.Request;

public class ReceiveTransactionRequest
{
    public StockTransactionType Type { get; set; }
    public Guid ProductId { get; set; } 
    public decimal Quantity { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; }
    public string Note { get; set; }
    public string PerformedBy { get; set; } = "System";
}
