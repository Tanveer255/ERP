using ERP.Data.DTO;
using ERP.Entity;
using ERP.Enum;
using ERP.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace ERP.Data.Request;

public record LogInRequest
{
    [ValidEmail]
    public string Email { get; set; }
    public string Password { get; set; }
    [Required]
    public string RecaptchaToken { get; set; }
}
public class LoginResponce
{
    public string Email { get; set; }
    public string TenantId { get; set; }
    public string FullName { get; set; }
    public List<AppFileDTO> FormFiles { get; set; }
}
public class LogoutResponce
{
    public string Token { get; set; }
}
public class GetProfileResponse
{
    public string Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CountryCode { get; set; }
    public List<AppFileDTO> FormFiles { get; set; }
}
public class GetSupportReqResponse
{
    public bool IsSupportReq { get; set; }
}
public class AdminUserResponse
{
    public string TenantId { get; set; }
    public string TenantName { get; set; }
    public string PartnerType { get; set; }
    public bool IsPartner { get; set; }
    public string Role { get; set; }
    public string Industry { get; set; }
    public string Email { get; set; }
    public string Country { get; set; }
    public bool IsSupported { get; set; }
    public DateTime? CreatedAt { get; set; }
}
public class RedirectRequest
{
    [ValidEmail]
    public string Email { get; set; }
    public string StaffUserEmail { get; set; }
}
public record LogOutRequest
{
    [ValidEmail]
    public string Email { get; set; }
}

public record UpDateUserRequest
{
    [Required]
    public string Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsTarifTool { get; set; }
    public bool IsPartner { get; set; }
    public string PartnerType { get; set; } = string.Empty;
    public string CouponCode { get; set; } = string.Empty;
    public string BussinessName { get; set; } = string.Empty;
    public string EmailToken { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public record SignUpRequest
{
    [ValidEmail]
    public string Email { get; set; }

    [ValidName]
    public string FirstName { get; set; } = string.Empty;

    [ValidName]
    public string LastName { get; set; } = string.Empty;

    [ValidPhone(Required = false)]
    public string PhoneNumber { get; set; } = string.Empty;

    [ValidCountryCode(Required = false)]
    public string CountryCode { get; set; } = string.Empty;

    [Required, MustBeTrue(ErrorMessage = "You must agree to the terms and conditions.")]
    public bool IsTermsAgreed { get; set; }

    public string CouponCode { get; set; } = string.Empty;

    [ValidCompanyName]
    public string BusinessName { get; set; } = string.Empty;

    [ValidPassword]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string RecaptchaToken { get; set; }
}
public record AdminUpRequest
{
    [ValidEmail]
    public string Email { get; set; }

    [ValidName]
    public string FirstName { get; set; } = string.Empty;

    [ValidName]
    public string LastName { get; set; } = string.Empty;

    [ValidPhone(Required = false)]
    public string PhoneNumber { get; set; } = string.Empty;

    [ValidCountryCode(Required = false)]
    public string CountryCode { get; set; } = string.Empty;

    [Required, MustBeTrue(ErrorMessage = "You must agree to the terms and conditions.")]

    [ValidPassword]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string RecaptchaToken { get; set; }
}

public class SignUpEmailDTO
{
    public string Token { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
}
public record UpdateUserRequest
{
    [ValidName]
    public string FirstName { get; set; } = string.Empty;

    [ValidName]
    public string LastName { get; set; } = string.Empty;

    [ValidPhone(Required = false)]
    public string PhoneNumber { get; set; } = string.Empty;

    [ValidCountryCode(Required = false)]
    public string CountryCode { get; set; } = string.Empty;
    public List<FormFileRequest> FormFiles { get; set; }
}
public class FormFileRequest
{
    public Guid Id { get; set; } = Guid.Empty;
    [MaxFileSize(1 * 1024 * 1024)] // 1 MB
    public IFormFile File { get; set; }
    public AttachmentType AttachmentType { get; set; }
}
public record UpdateUserResponse
{
    public string Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<AppFile> FormFiles { get; set; }
}