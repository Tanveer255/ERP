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
