using ERP.Entity.Auth;
using ERP.Enum;
using ERP.Repository;
using ERP.Repository.Auth;
using ERP.Service.Common;

namespace ERP.Service.Auth;

/// <summary>
/// This is the IAddressTypeService interface which inherits all properties of ICrudService and has some methods given below.
/// </summary>
public interface IAddressTypeService : ICrudService<Address>
{
    /// <summary>
    /// Creates a set of default addresses for the company identified by <paramref name="companyId"/>, 
    /// associated with the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company to create default addresses for.</param>
    /// <param name="tenantId">The unique identifier of the tenant the company belongs to.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    Task CreateDefaultAddressesAsync(Guid companyId, string tenantId);
}
/// <summary>
/// Initializes a new instance of the <see cref="AccessTokenService"/> class.
/// </summary>
/// <param name="unitOfWork">Parameter of AddressType service class constructor use to manage the AddressType service repository.</param>
/// <param name="addressTypeRepository">Parameter of AddressType service class constructor.</param>

public class AddressTypeService(
    IUnitOfWork unitOfWork,
    IAddressTypeRepository addressTypeRepository
    ) : CrudService<Address>(addressTypeRepository, unitOfWork), IAddressTypeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAddressTypeRepository _addressTypeRepository = addressTypeRepository;

    /// <summary>
    /// Creates a set of default addresses for the company identified by <paramref name="companyId"/>, 
    /// associated with the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company to create default addresses for.</param>
    /// <param name="tenantId">The unique identifier of the tenant the company belongs to.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    public async Task CreateDefaultAddressesAsync(Guid companyId, string tenantId)
    {
        var defaultAddress = new Address
        {
            AddressLine = string.Empty,
            AddressLine2 = string.Empty,
            TownLocality = string.Empty,
            CityRegion = string.Empty,
            State = string.Empty,
            PostalZipCode = string.Empty,
            TenantId = tenantId,
            CountryId = "US",
            CountryName = "US"
        };

        var addresses = new List<Address>
        {
            new Address
            {
                CompanyId = companyId,
                Type = nameof(AddressTypeEnum.Primary),
                AddressLine = defaultAddress.AddressLine,
                AddressLine2 = defaultAddress.AddressLine2,
                TownLocality = defaultAddress.TownLocality,
                CityRegion = defaultAddress.CityRegion,
                State = defaultAddress.State,
                PostalZipCode = defaultAddress.PostalZipCode,
                CountryId = defaultAddress.CountryId,
                CountryName = defaultAddress.CountryName,
                TenantId = tenantId
            },
            new Address
            {
                CompanyId = companyId,
                Type = nameof(AddressTypeEnum.Secondary),
                AddressLine = defaultAddress.AddressLine,
                AddressLine2 = defaultAddress.AddressLine2,
                TownLocality = defaultAddress.TownLocality,
                CityRegion = defaultAddress.CityRegion,
                State = defaultAddress.State,
                PostalZipCode = defaultAddress.PostalZipCode,
                CountryId = defaultAddress.CountryId,
                CountryName = defaultAddress.CountryName,
                TenantId = tenantId
            },
            new Address
            {
                CompanyId = companyId,
                Type = nameof(AddressTypeEnum.Invoice),
                AddressLine = defaultAddress.AddressLine,
                AddressLine2 = defaultAddress.AddressLine2,
                TownLocality = defaultAddress.TownLocality,
                CityRegion = defaultAddress.CityRegion,
                State = defaultAddress.State,
                PostalZipCode = defaultAddress.PostalZipCode,
                CountryId = defaultAddress.CountryId,
                CountryName = defaultAddress.CountryName,
                TenantId = tenantId
            }
        };

        await _addressTypeRepository.Add(addresses);
        await _unitOfWork.CommitAsync();
    }

}
