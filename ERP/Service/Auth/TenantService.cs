using ERP.Entity.Auth;
using ERP.Enum;
using ERP.Repository;
using ERP.Repository.Auth;
using ERP.Service.Common;
using System.Text.RegularExpressions;

namespace ERP.Service.Auth;

/// <summary>
/// This is the ITenantService interface which inherits all properties of ICrudService and has some methods given below.
/// </summary>
public interface ITenantService : ICrudService<Tenant>
{
    Task<Tenant> CreateTenant(string businessName);
}
/// <summary>
/// Initializes a new instance of the <see cref="TenantService"/> class.
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="tenantRepository"></param>
/// <param name="logger"></param>
public class TenantService(
    IUnitOfWork unitOfWork,
    ITenantRepository tenantRepository,
    ILogger<TenantService> logger
    ) : CrudService<Tenant>(tenantRepository, unitOfWork), ITenantService
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<TenantService> _logger = logger;
    /// <summary>
    /// Method of Tenant Service to create a tenant.
    /// </summary>
    /// <param name="businessName"></param>
    /// <param name="couponCode"></param>
    /// <returns></returns>
    public async Task<Tenant> CreateTenant(string businessName)
    {
        string tenantId = string.Empty;

        string tenantIdTemp = RemoveSpecialCharacters(businessName)
                                .Replace(" ", string.Empty)
                                .ToLower();

        long timestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Thread-safe random number
        int random = Random.Shared.Next(1000, 9999);

        // Ensure tenantId has a meaningful length
        string prefix = tenantIdTemp.Length <= 3
            ? tenantIdTemp
            : tenantIdTemp.Substring(0, 3);

        tenantId = $"{prefix}{timestampMs}{random}";


        // Create a Tenant object
        var tenant = new Tenant
        {
            TenantId = tenantId,
            TenantName = businessName,
            TenantType = nameof(TenantType.Customer),
            TenantStatus = nameof(UserStatus.Active),
        };
        await _tenantRepository.Add(tenant);
        await _unitOfWork.CommitAsync();
        return tenant;
    }
    /// <summary>
    /// Method of Tenant Service to remove special character from string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private String RemoveSpecialCharacters(string str)
    {
        try
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            // This regex matches anything that is not a letter, number, or underscore
            return Regex.Replace(str, @"[^A-Za-z0-9_]", string.Empty);
        }
        catch (Exception)
        {
            _logger.LogError($"Something went wrong. Please try again later.");
            return string.Empty;
        }

    }
}

