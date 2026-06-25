namespace Identity.Domain.Entities;

public class Tenant : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.IAggregateRoot, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId => Id;
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public static Tenant Create(string name, string code) =>
        new() { Name = name, Code = code };
}

public class ApplicationUser : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.IAggregateRoot, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    private ApplicationUser() { }

    public static ApplicationUser Create(Guid tenantId, string email, string passwordHash, string firstName, string lastName) =>
        new()
        {
            TenantId = tenantId,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName
        };

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class ApplicationRole : BuildingBlocks.Domain.AuditableEntity, BuildingBlocks.Domain.IAggregateRoot, BuildingBlocks.Domain.ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private ApplicationRole() { }

    public static ApplicationRole Create(Guid tenantId, string name) =>
        new() { TenantId = tenantId, Name = name, NormalizedName = name.ToUpperInvariant() };
}

public class Permission : BuildingBlocks.Domain.Entity
{
    public string Code { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private Permission() { }

    public static Permission Create(string code, string module, string description) =>
        new() { Code = code, Module = module, Description = description };
}

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public class RefreshToken : BuildingBlocks.Domain.Entity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => RevokedAtUtc is null && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAtUtc) =>
        new() { UserId = userId, Token = token, ExpiresAtUtc = expiresAtUtc };

    public void Revoke() => RevokedAtUtc = DateTime.UtcNow;
}
