using ERP.Infrastructure.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ERP.Entity.Auth;

public class AddressTypeDTO
{
    [Required, NotEmptyGuid]
    public Guid Id { get; set; }
    [Trim]
    public string CountryName { get; set; } = string.Empty;
    [Trim]
    public string AddressLine { get; set; } = string.Empty;
    [Trim]
    public string CityRegion { get; set; } = string.Empty;
    [Trim]
    public string PostalZipCode { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string AddressLine2 { get; set; } = string.Empty;
    public string TownLocality { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CountryId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string PhoneCountryCode { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public Address MapDtoToModel(AddressTypeDTO dto)
    {
        return new Address()
        {
            CompanyId = dto.CompanyId,
            AddressLine = dto.AddressLine,
            AddressLine2 = dto.AddressLine2,
            TownLocality = dto.TownLocality,
            CityRegion = dto.CityRegion,
            State = dto.State,
            PostalZipCode = dto.PostalZipCode,
            CountryId = dto.CountryId,
            CountryName = dto.CountryName,
            Type = dto.Type,
            PhoneCountryCode = dto.PhoneCountryCode,
            PhoneNo = dto.PhoneNo,
        };
    }

    public AddressTypeDTO MapModelToDto(Address entity)
    {
        return new AddressTypeDTO()
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            AddressLine = entity.AddressLine,
            AddressLine2 = entity.AddressLine2,
            TownLocality = entity.TownLocality,
            CityRegion = entity.CityRegion,
            State = entity.State,
            PostalZipCode = entity.PostalZipCode,
            CountryId = entity.CountryId,
            CountryName = entity.CountryName,
            Type = entity.Type,
            PhoneCountryCode = entity.PhoneCountryCode,
            PhoneNo = entity.PhoneNo,
        };
    }

    public void MapEntityToDto(Address dto)
    {
        CompanyId = dto.CompanyId;
        AddressLine = dto.AddressLine;
        AddressLine2 = dto.AddressLine2;
        TownLocality = dto.TownLocality;
        CityRegion = dto.CityRegion;
        State = dto.State;
        PostalZipCode = dto.PostalZipCode;
        CountryId = dto.CountryId;
        CountryName = dto.CountryName;
        Type = dto.Type;
        PhoneCountryCode = dto.PhoneCountryCode;
        PhoneNo = dto.PhoneNo;
    }
}
