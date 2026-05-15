using ERP.Data.DTO;

namespace ERP.Data.Response;

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
