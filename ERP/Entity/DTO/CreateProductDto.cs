namespace ERP.Entity.DTO;

public class CreateProductDto
{
    public string Name { get; set; }
    public string Unit { get; set; }
    public decimal Quantity { get; set; }     
    public decimal SalePrice { get; set; }     
    public decimal UnitCost { get; set; }     
    public decimal DiscountAmount { get; set; }     
    public decimal DiscountPercentage { get; set; }     
    public decimal TaxPercentage { get; set; }     
    public bool IsManufactured { get; set; }
    
}

public class UpdateProductDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public bool IsManufactured { get; set; }
}
