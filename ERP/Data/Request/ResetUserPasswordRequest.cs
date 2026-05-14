namespace ERP.Data.Request;

public class SendResetPasswordEmailRequest
{
    //[ValidEmail]
    public string Email { get; set; }
}

public class ResetUserPasswordRequest
{
    //[ValidEmail]
    public string Email { get; set; }
    //[ValidPassword]
    public string Password { get; set; }
}

public class UpdateUserStatusRequest
{
    public string TenantId { get; set; }
    //[ValidUserStatus]
    public string Status { get; set; }
}

public class UpdateUserSubscriptionDetailsRequest
{
    public string TenantId { get; set; }
    public bool IsPartner { get; set; }
    //[ValidPartnerType]
    public string PartnerType { get; set; }
}

public class GetUserDetailsRequest
{
    public string TenantId { get; set; }
}

public class ExportAllUsersRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid UserId { get; set; }
}
