using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Product;

public class ProductService
{
    private readonly ManufacturingDbContext _context;
    public ProductService(ManufacturingDbContext manufacturingDbContext )
    {
        _context = manufacturingDbContext;
    }
    public async Task<ResultDTO<ProductEntity>> GetProductById(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return ResultDTO<ProductEntity>.Failure($"Product {productId} not found.");

        return ResultDTO<ProductEntity>.Success(product);
    }
}
