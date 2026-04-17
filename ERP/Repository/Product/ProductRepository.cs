using ERP.Entity.Product;

namespace ERP.Repository.Product;

public interface IProductRepository : IRepository<ProductEntity>
{
}

public class ProductRepository(
    IUnitOfWork unitOfWork,
    ILogger<ProductRepository> logger
    ) : Repository<ProductEntity>(unitOfWork, logger), IProductRepository
{ 
}