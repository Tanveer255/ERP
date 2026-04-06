using System.ComponentModel.DataAnnotations;

namespace ERP.Data.DTO.Document;

public class SalesOrderItemDto
{
    [Required(ErrorMessage = "ProductId is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
    public decimal QuantityRequested { get; set; }
}
