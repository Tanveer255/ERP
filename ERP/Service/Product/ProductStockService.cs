using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity.Product;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Product;

public class ProductStockService
{
    private ManufacturingDbContext _context;
    public ProductStockService(ManufacturingDbContext context)
    {
        _context = context;
    }
    public async Task<ResultDTO<ProductStock>> GetStockStockByProductId(Guid productId, CancellationToken cancellationToken = default)
    {
        var result = await _context.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);
        if (result ==null)
        {
                return ResultDTO<ProductStock>.Failure($"Product {productId} not found.");
        }
        return ResultDTO<ProductStock>.Success(result);
    }
}
