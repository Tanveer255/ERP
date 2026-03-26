using System.ComponentModel.DataAnnotations;

namespace ERP.Entity.DTO.Document;

public class SalesOrderItemDto
{
    [Required(ErrorMessage = "ProductId is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
    public decimal Quantity { get; set; }
}
