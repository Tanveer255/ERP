using ERP.Data.Request;
using ERP.Entity;
using ERP.Entity.Auth;
using ERP.Infrastructure;
using ERP.Infrastructure.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ERP.Data.DTO.Auth;

public class CompanyDTO
{
    [Required, NotEmptyGuid]
    public new Guid Id { get; set; }
    [ValidCompanyName]
    public new string CompanyName { get; set; }
    public List<AddressTypeDTO> AddressTypes { get; set; }
    public FormFileRequest FormFile { get; set; }
    public AppFile CompanyLogo { get; set; }
    public Guid UserId { get; set; }
    public string TenantId { get; set; } = string.Empty;
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
    public bool IsPartner { get; set; }
    public string PartnerType { get; set; } = string.Empty;
    public Company MapDtoToModel(CompanyDTO dto)
    {
        return new Company()
        {
            Id = dto.Id,
            CompanyName = dto.CompanyName,
            WebSite = dto.WebSite,
            TaxIDorVATNo = dto.TaxIDorVATNo,
            MobileCountryCode = dto.MobileCountryCode,
            MobileNo = dto.MobileNo,
            PhoneCountryCode = dto.PhoneCountryCode,
            PhoneNo = dto.PhoneNo,
            TurnoverAmount = dto.TurnoverAmount,
            TurnoverCcy = dto.TurnoverCcy,
            BusinessYear = dto.BusinessYear,
            LogoSaveId = dto.LogoSaveId,
            TimeZoneId = dto.TimeZoneId,
            NumberOfEmployees = dto.NumberOfEmployees,
            ProcessUser = dto.ProcessUser,
            Comment = dto.Comment,
            IsNewSignUp = dto.IsNewSignUp,
            RegistrationNo = dto.RegistrationNo,
            PrimaryBusinessSector = dto.PrimaryBusinessSector,
            CompanyEmail = dto.CompanyEmail,
            GeneralEmail = dto.GeneralEmail,
            EoriNumber = dto.EoriNumber,
            IsPartner = dto.IsPartner,
            TenantId = dto.TenantId,
            PartnerType = dto.PartnerType
        };
    }

    public CompanyDTO MapModelToDto(Company entity)
    {
        return new CompanyDTO()
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            WebSite = entity.WebSite,
            TaxIDorVATNo = entity.TaxIDorVATNo,
            MobileCountryCode = entity.MobileCountryCode,
            MobileNo = entity.MobileNo,
            PhoneCountryCode = entity.PhoneCountryCode,
            PhoneNo = entity.PhoneNo,
            TurnoverAmount = entity.TurnoverAmount,
            TurnoverCcy = entity.TurnoverCcy,
            BusinessYear = entity.BusinessYear,
            LogoSaveId = entity.LogoSaveId,
            TimeZoneId = entity.TimeZoneId,
            NumberOfEmployees = entity.NumberOfEmployees,
            ProcessUser = entity.ProcessUser,
            Comment = entity.Comment,
            IsNewSignUp = entity.IsNewSignUp,
            RegistrationNo = entity.RegistrationNo,
            PrimaryBusinessSector = entity.PrimaryBusinessSector,
            CompanyEmail = entity.CompanyEmail,
            GeneralEmail = entity.GeneralEmail,
            EoriNumber = entity.EoriNumber,
            IsPartner = entity.IsPartner,
            TenantId = entity.TenantId,
            PartnerType = entity.PartnerType
        };
    }

    public void MapEntityToDto(Company entity)
    {
        CompanyName = entity.CompanyName;
        WebSite = entity.WebSite;
        TaxIDorVATNo = entity.TaxIDorVATNo;
        MobileCountryCode = entity.MobileCountryCode;
        MobileNo = entity.MobileNo;
        PhoneCountryCode = entity.PhoneCountryCode;
        PhoneNo = entity.PhoneNo;
        TurnoverAmount = entity.TurnoverAmount;
        TurnoverCcy = entity.TurnoverCcy;
        BusinessYear = entity.BusinessYear;
        LogoSaveId = entity.LogoSaveId;
        TimeZoneId = entity.TimeZoneId;
        NumberOfEmployees = entity.NumberOfEmployees;
        ProcessUser = entity.ProcessUser;
        Comment = entity.Comment;
        IsNewSignUp = entity.IsNewSignUp;
        RegistrationNo = entity.RegistrationNo;
        PrimaryBusinessSector = entity.PrimaryBusinessSector;
        CompanyEmail = entity.CompanyEmail;
        GeneralEmail = entity.GeneralEmail;
        EoriNumber = entity.EoriNumber;
        IsPartner = entity.IsPartner;
        PartnerType = entity.PartnerType;
    }
}
