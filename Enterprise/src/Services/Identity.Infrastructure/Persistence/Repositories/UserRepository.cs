using BuildingBlocks.Infrastructure.Persistence;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public class UserRepository(IdentityDbContext context) : EfRepository<ApplicationUser>(context), IUserRepository
{
    private IdentityDbContext Db => (IdentityDbContext)Context;

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Db.Users.FirstOrDefaultAsync(x => x.Email == email.ToLowerInvariant(), cancellationToken);

    public Task<ApplicationUser?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Db.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x.RolePermissions).ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}

public class RefreshTokenRepository(IdentityDbContext context) : EfRepository<RefreshToken>(context), IRefreshTokenRepository
{
    private IdentityDbContext Db => (IdentityDbContext)Context;

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        Db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
}

public class TenantRepository(IdentityDbContext context) : EfRepository<Tenant>(context), ITenantRepository
{
    private IdentityDbContext Db => (IdentityDbContext)Context;

    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Db.Tenants.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
}

public class IdentityUnitOfWork(IdentityDbContext context) : UnitOfWork(context), BuildingBlocks.Domain.Repositories.IUnitOfWork;
