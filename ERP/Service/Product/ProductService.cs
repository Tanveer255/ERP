using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity.Product;
using ERP.Repository;
using ERP.Repository.Product;
using ERP.Service.Common;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Product;
public interface IProductService : ICrudService<ProductEntity>
{ 
    Task<ResultDTO<ProductEntity>> GetProductById(Guid productId);
    Task<bool> IsStockItemAsync(Guid productId, CancellationToken cancellationToken = default);
}
public class ProductService(IProductRepository productRepository,IUnitOfWork unitOfWork)
    : CrudService<ProductEntity>(productRepository, unitOfWork), IProductService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IProductRepository _productRepository = productRepository;
    public async Task<ResultDTO<ProductEntity>> GetProductById(Guid productId)
    {
        var product = await _unitOfWork.Context.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return ResultDTO<ProductEntity>.Failure($"Product {productId} not found.");

        return ResultDTO<ProductEntity>.Success(product);
    }
    public async Task<bool> IsStockItemAsync( Guid productId, CancellationToken cancellationToken = default)
    {
        var productInfo = await _unitOfWork.Context.Products
            .Where(p => p.Id == productId)
            .Select(p => new
            {
                p.IsManufactured,
                HasBOM = _unitOfWork.Context.BillOfMaterials.Any(b => b.ProductId == p.Id)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return productInfo != null && !productInfo.IsManufactured && !productInfo.HasBOM;
    }

}
