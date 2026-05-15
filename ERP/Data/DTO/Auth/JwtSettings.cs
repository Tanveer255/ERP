namespace ERP.Data.DTO.Auth;

public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryTime { get; set; }
}
