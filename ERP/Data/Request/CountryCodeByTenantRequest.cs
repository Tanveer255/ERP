namespace ERP.Data.Request;

public class CountryCodeByTenantRequest
{
    public string TenantId { get; set; }
}
public class CountryCodeByTenantResponse
{
    public string CountryCode { get; set; }
}
