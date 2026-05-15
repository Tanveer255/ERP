using ERP.Data.DTO.Auth;
using ERP.Data.Request;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ERP.Service.Auth;

public interface IJwtAuthenticationService
{
    /// <summary>
    /// Generate token when login user
    /// </summary>
    /// <param name="generateTokenRequest"></param>
    /// <returns></returns>
    Task<string> GenerateTokenAsync(GenerateTokenRequest generateTokenRequest);
    /// <summary>
    /// referesh Token 
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <returns></returns>
    Task<RefreshTokenRequest> RefreshTokenAsync(GenerateTokenRequest tokenRequest);
}

public class JwtAuthenticationService : IJwtAuthenticationService
{
    private readonly JwtSettings _jwtSettings;

    public JwtAuthenticationService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
    /// <summary>
    /// Generate token when login user
    /// </summary>
    /// <param name="generateTokenRequest"></param>
    /// <returns></returns>
    public async Task<string> GenerateTokenAsync(GenerateTokenRequest request)
    {
        var claims = new[]
        {
                new Claim(ClaimTypes.Email, request.Email),
                new Claim("TenantId", request.TenantId),
                new Claim(ClaimTypes.NameIdentifier,request.UserId.ToString()),
                new Claim("SettingId", request.SettingId.ToString()),
                new Claim(ClaimTypes.Role, request.Role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials
        );
        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
    /// <summary>
    /// referesh Token 
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <returns></returns>
    public async Task<RefreshTokenRequest> RefreshTokenAsync(GenerateTokenRequest tokenRequest)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, tokenRequest.Email),
            new Claim(ClaimTypes.NameIdentifier, tokenRequest.UserId.ToString())
        };

        var newRefreshToken = GenerateTokenAsync(tokenRequest);

        var token = new RefreshTokenRequest
        {
            RefreshToken = await newRefreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(_jwtSettings.RefreshTokenExpiryTime)
        };

        return token;
    }
}