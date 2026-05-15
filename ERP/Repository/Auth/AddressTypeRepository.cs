using ERP.Data.Request;
using ERP.Entity.Auth;
using ERP.Enum;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Auth;

public interface IAddressTypeRepository : IRepository<Address>
{
    /// <summary>
    /// GetByCompanyIdAsync
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task<IEnumerable<Address>> GetByCompanyIdAsync(Guid companyId, string tenantId);
    Task<CountryCodeByTenantResponse> GetCountryCodeByTenantIdAsync(string tenantId);
}
public class AddressTypeRepository(
   IUnitOfWork unitOfWork,
   ILogger<AddressTypeRepository> logger
   ) : Repository<Address>(unitOfWork, logger), IAddressTypeRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    /// <summary>
    /// GetByCompanyIdAsync
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Address>> GetByCompanyIdAsync(Guid companyId, string tenantId)
    {
        var addresses = await Get();
        return addresses.Where(address => address.CompanyId.Equals(companyId) && address.TenantId == tenantId);
    }
    public async Task<CountryCodeByTenantResponse?> GetCountryCodeByTenantIdAsync(string tenantId)
    {
        var countryCode = await _unitOfWork.Context.Addresses
            .Where(a => a.TenantId == tenantId && a.Type == nameof(AddressCategory.Primary))
            .Select(a => a.CountryName)
            .FirstOrDefaultAsync();

        return countryCode is null
            ? null
            : new CountryCodeByTenantResponse { CountryCode = countryCode };
    }


}
