using System.ComponentModel.DataAnnotations;

namespace ERP.Entity.Auth;

public class Address : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string TownLocality { get; set; } = string.Empty;
    public string CityRegion { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalZipCode { get; set; } = string.Empty;
    public string CountryId { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string PhoneCountryCode { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string TenantId { get; set; } = string.Empty;
}
