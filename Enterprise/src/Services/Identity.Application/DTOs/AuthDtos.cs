namespace Identity.Application.DTOs;

public record AuthTokenDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc);
public record UserDto(Guid Id, Guid TenantId, string Email, string FullName, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
public record LoginRequest(string Email, string Password);
public record RegisterRequest(Guid TenantId, string Email, string Password, string FirstName, string LastName);
public record RefreshTokenRequest(string RefreshToken);
