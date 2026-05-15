using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ERP.Repository.Auth;

/// <summary>
/// Defines repository operations specific to the <see cref="Company"/> entity.
/// Inherits basic CRUD operations from <see cref="IRepository{Company}"/>.
/// </summary>
public interface ICompanyRepository : IRepository<Company>
{
    /// <summary>
    /// Checks whether a record with the specified <paramref name="saveId"/> exists.
    /// </summary>
    /// <param name="saveId">The unique identifier of the record to check.</param>
    /// <returns>True if the record exists; otherwise, false.</returns>
    Task<bool> IsExist(Guid saveId, string tenantId);

    /// <summary>
    /// Retrieves the company details associated with the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <returns>The company details as a <see cref="CompanyDTO"/>.</returns>
    Task<CompanyDTO> GetByTenantIdAsync(string tenantId);

    /// <summary>
    /// Retrieves a list of companies associated with the specified tenant IDs
    /// </summary>
    /// <param name="tenantIds">A list of tenant IDs for which company data is to be retrieved.</param>
    /// <returns>A <see cref="List{CompanyAndSubscriptionDetailsDTO}"/> containing company details.</returns>
    Task<List<CompanyAndSubscriptionDetailsDTO>> GetCompaniesByListOfTenantId(List<string> tenantIds);

    /// <summary>
    /// Retrieves a list of companies and their addresses associated with the specified tenant IDs
    /// </summary>
    /// <param name="tenantIds">A list of tenant IDs for which company and address data is to be retrieved.</param>
    /// <returns>A <see cref="List{AddressAndSubscriptionDetailsDTO}"/> containing company, address information.</returns>
    Task<List<AddressAndSubscriptionDetailsDTO>> GetCompaniesAndAddressesByListOfTenantId(List<string> tenantIds);
}

/// <summary>
/// Provides implementation for company-specific data access operations.
/// Inherits basic CRUD functionality from <see cref="Repository{Company}"/> and implements <see cref="ICompanyRepository"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work for managing transactions.</param>
/// <param name="logger">The logger instance for logging operations.</param>
public class CompanyRepository(IUnitOfWork unitOfWork, ILogger<CompanyRepository> logger) : Repository<Company>(unitOfWork, logger), ICompanyRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CompanyRepository> _logger = logger;

    /// <summary>
    /// Checks whether a record with the specified <paramref name="saveId"/> exists.
    /// </summary>
    /// <param name="saveId">The unique identifier of the record to check.</param>
    /// <returns>True if the record exists; otherwise, false.</returns>
    public async Task<bool> IsExist(Guid saveId, string tenantId)
    {
        return await GetAllReadOnly().AnyAsync(company => company.Id == saveId && company.TenantId == tenantId && !company.IsDeleted);
    }

    /// <summary>
    /// Retrieves the company details associated with the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <returns>The company details as a <see cref="CompanyDTO"/>.</returns>
    public async Task<CompanyDTO> GetByTenantIdAsync(string tenantId)
    {
        return await GetAllReadOnly()
                        .Where(company => company.TenantId.Equals(tenantId) && !company.IsDeleted)
                        .Select(company => new CompanyDTO().MapModelToDto(company))
                        .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves a list of companies associated with the specified tenant IDs
    /// </summary>
    /// <param name="tenantIds">A list of tenant IDs for which company data is to be retrieved.</param>
    /// <returns>A <see cref="List{CompanyAndSubscriptionDetailsDTO}"/> containing company details.</returns>
    public async Task<List<CompanyAndSubscriptionDetailsDTO>> GetCompaniesByListOfTenantId(List<string> tenantIds)
    {
        return await GetAllReadOnly()
            .Join(_unitOfWork.Context.Users, u => u.TenantId, user => user.TenantId, (company, user) => new { company, user })
                        .Where(x => tenantIds.Contains(x.company.TenantId) && !x.company.IsDeleted)
                        .Select(x => new CompanyAndSubscriptionDetailsDTO
                        {
                            TenantId = x.company.TenantId,
                            CompanyName = x.company.CompanyName,
                            Email = x.user.Email
                        })
                        .ToListAsync();
    }

    /// <summary>
    /// Retrieves a list of companies and their addresses associated with the specified tenant IDs
    /// </summary>
    /// <param name="tenantIds">A list of tenant IDs for which company and address data is to be retrieved.</param>
    /// <returns>A <see cref="List{AddressAndSubscriptionDetailsDTO}"/> containing company, address information.</returns>
    public async Task<List<AddressAndSubscriptionDetailsDTO>> GetCompaniesAndAddressesByListOfTenantId(List<string> tenantIds)
    {
        return await GetAllReadOnly()
            .Join(_unitOfWork.Context.Users, u => u.TenantId, user => user.TenantId, (company, user) => new { company, user })
            .Join(_unitOfWork.Context.Addresses.Where(a => a.Type == nameof(AddressTypeEnum.Primary)),
                  a => a.user.TenantId, address => address.TenantId, (x, address) => new { x.company, x.user, address })
                        .Where(x => tenantIds.Contains(x.company.TenantId) && !x.company.IsDeleted)
                        .Select(x => new AddressAndSubscriptionDetailsDTO
                        {
                            TenantId = x.company.TenantId,
                            CompanyName = x.company.CompanyName,
                            AddressLine1 = x.address.AddressLine,
                            AddressLine2 = x.address.AddressLine2,
                            TownLocality = x.address.TownLocality,
                            CityRegion = x.address.CityRegion,
                            State = x.address.State,
                            Country = x.address.CountryName
                        })
                        .ToListAsync();
    }
}
