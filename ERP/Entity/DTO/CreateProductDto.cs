namespace ERP.Entity.DTO;

public class CreateProductDto
{
    public string Name { get; set; }
    public string Unit { get; set; }
    public decimal Quantity { get; set; }     
    public bool IsManufactured { get; set; }
    
}

public class UpdateProductDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public bool IsManufactured { get; set; }
}
