namespace Identity.Application.Interfaces;

public interface ITokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(Guid userId, Guid tenantId, string email, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
