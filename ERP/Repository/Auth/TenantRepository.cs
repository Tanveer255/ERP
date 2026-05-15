using ERP.Entity.Auth;

namespace ERP.Repository.Auth;

/// <summary>
/// Tenant Repository interface.
/// </summary>

public interface ITenantRepository : IRepository<Tenant>
{
}
/// <summary>
/// Initializes a new instance of the <see cref="TenantRepository"/> class.
/// </summary>
/// <param name="unitOfWork">is an object of IUnitOfWork.</param>
/// <param name="logger">is an object of ILogger.</param>
public class TenantRepository(
    IUnitOfWork unitOfWork,
    ILogger<TenantRepository> logger
    ) : Repository<Tenant>(unitOfWork, logger), ITenantRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<TenantRepository> _logger = logger;


}
