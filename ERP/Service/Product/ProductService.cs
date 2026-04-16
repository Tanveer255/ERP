using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity.Product;
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
    public async Task<bool> IsStockItemAsync( Guid productId, CancellationToken cancellationToken = default)
    {
        var productInfo = await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => new
            {
                p.IsManufactured,
                HasBOM = _context.BillOfMaterials.Any(b => b.ProductId == p.Id)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return productInfo != null && !productInfo.IsManufactured && !productInfo.HasBOM;
    }

}
