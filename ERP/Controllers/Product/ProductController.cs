using ERP.Data;
using ERP.Data.DTO.Product;
using ERP.Entity;
using ERP.Entity.Product;
using ERP.Repository;
using ERP.Service;
using ERP.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers.Product;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductService productService,IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;


    // GET: api/product
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _unitOfWork.Context.Products
            .Include(p => p.BOMs) // eager load BOMs if needed
            .ToListAsync();
        return Ok(products);
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _unitOfWork.Context.Products
            .Include(p => p.BOMs)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }
    /// <summary>
    /// CreateProduct
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
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

       await _productService.Add(product);
        await _unitOfWork.CommitAsync();

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

        _unitOfWork.Context.ProductStocks.Add(stock);
        _unitOfWork.Context.Prices.Add(price);

        await _unitOfWork.CommitAsync();

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
        var product = await _unitOfWork.Context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Code = dto.Code;
        product.Name = dto.Name;
        product.Unit = dto.Unit;
        product.IsManufactured = dto.IsManufactured;

        await _unitOfWork.CommitAsync();

        return NoContent();
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _unitOfWork.Context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        _unitOfWork.Context.Products.Remove(product);
        await _unitOfWork.CommitAsync();

        return NoContent();
    }
   
}