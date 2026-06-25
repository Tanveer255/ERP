using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IUserRepository : BuildingBlocks.Domain.Repositories.IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository : BuildingBlocks.Domain.Repositories.IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
}

public interface ITenantRepository : BuildingBlocks.Domain.Repositories.IRepository<Tenant>
{
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
