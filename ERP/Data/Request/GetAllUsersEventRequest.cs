namespace ERP.Data.Request;

public class GetAllUsersEventRequest : PagedRequest
{
    public Guid UserId { get; set; }
    public string FilterType { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
public class ExportAllUsersEventRequest : PagedRequest
{
    public Guid UserId { get; set; }
    public string FilterType { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
public abstract class PagedRequest
{
    public int PageSize => 10;
    public int PageIndex { get; set; }
    public string Query { get; set; } = string.Empty;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
public class GetAllUsersEventResponse
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
    public DateTime? LastActivity { get; set; }
}
public class ExportAllUsersEventResponse
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
    public DateTime? LastActivity { get; set; }
}


