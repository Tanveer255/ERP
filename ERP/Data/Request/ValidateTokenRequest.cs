using ERP.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace ERP.Data.Request;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }
}
public class RefreshTokenResponse
{
    public string RefreshToken { get; set; } = string.Empty;
}
public class GenerateTokenRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid SettingId { get; set; }
    public string Role { get; set; } = string.Empty;
}
public class RefreshMultiDeviceTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
public class RefreshMultiDeviceTokenResponse
{
    public string AcessToken { get; set; } = string.Empty;
    public string NewToken { get; set; } = string.Empty;
}
public class ValidateTokenRequest
{
    [ValidEmail]
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
public class ValidateTokenResponse
{
    public string Email { get; set; } = string.Empty;
}
public class ResendEmailConfirmation
{
    [Required]
    [ValidEmail]
    public string Email { get; set; } = string.Empty;
}
public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; }
    [ValidPassword]
    public string NewPassword { get; set; }
    [Compare(nameof(NewPassword), ErrorMessage = "Confirm password must match new password.")]
    public string ConfirmPassword { get; set; }
}
public class ForgotPasswordEmailRequest
{
    [ValidEmail]
    public string Email { get; set; }
    public string RecaptchaToken { get; set; }

}
public class ResetPasswordRequest
{
    [ValidPassword]
    public string Password { get; set; }
    public string Token { get; set; }
    [ValidEmail]
    public string Email { get; set; }
}
