using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

[Table(nameof(Company))]
public class Company : BaseEntity
{
    [Required, MaxLength(20)]
    public string TenantId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string WebSite { get; set; } = string.Empty;
    public string TaxIDorVATNo { get; set; } = string.Empty;
    public string MobileCountryCode { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string PhoneCountryCode { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string TurnoverAmount { get; set; } = string.Empty;
    public string TurnoverCcy { get; set; } = string.Empty;
    public string BusinessYear { get; set; } = string.Empty;
    public string LogoSaveId { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = string.Empty;
    public string NumberOfEmployees { get; set; } = string.Empty;
    public string ProcessUser { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsNewSignUp { get; set; }
    public string RegistrationNo { get; set; } = string.Empty;
    public string PrimaryBusinessSector { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string GeneralEmail { get; set; } = string.Empty;
    public string EoriNumber { get; set; } = string.Empty;
    public bool IsPartner { get; set; } = false;
    [RegularExpression("Fedex", ErrorMessage = "Partner type must be 'Fedex'.")]
    public string PartnerType { get; set; } = string.Empty;
}
