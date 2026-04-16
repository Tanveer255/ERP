using ERP.Data;
using ERP.Data.DTO.Product;
using ERP.Entity;
using ERP.Entity.Product;
using ERP.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers.Product;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ManufacturingDbContext _context;

    public ProductController(ManufacturingDbContext context)
    {
        _context = context;
    }

    // GET: api/product
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.BOMs) // eager load BOMs if needed
            .ToListAsync();
        return Ok(products);
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.BOMs)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // POST: api/product
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        var product = new ProductEntity
        {
            Code = Helper.GenerateCode(),
            Name = dto.Name,
            Unit = dto.Unit,
            UnitCost = dto.UnitCost,
            IsManufactured = dto.IsManufactured
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(); // important

        var stock = new ProductStock
        {
            ProductId = product.Id,
            QuantityAvailable = dto.QuantityRequested,
            QuantityReserved = 0.0m,
            QuantityInProduction = 0.0m,
            QuantityQuarantined = 0.0m,
            QuantityRejected = 0.0m,
            QuantityExpired = 0.0m,
            Warehouse = "None",
            Zone = "None",
            Aisle = "None",
            Rack = "None",
            Shelf = "None",
            LastUpdated = DateTime.UtcNow
        };

        var price = new Price
        {
            ProductId = product.Id,
            SalePrice = dto.SalePrice,
            DiscountAmount = dto.DiscountAmount,
            DiscountPercentage = dto.DiscountPercentage,
            TaxPercentage = dto.TaxPercentage,
            Currency = "USD",
            CreatedAt = DateTime.UtcNow
        };

        price.FinalPrice = Helper.GetFinalPrice(price);

        _context.ProductStocks.Add(stock);
        _context.Prices.Add(price);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
        {
            product.Id,
            product.Name,
            product.Code
        });
    }

    // PUT: api/product/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Code = dto.Code;
        product.Name = dto.Name;
        product.Unit = dto.Unit;
        product.IsManufactured = dto.IsManufactured;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
   
}