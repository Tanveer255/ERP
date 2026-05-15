using ERP.Data.DTO;
using ERP.Entity;
using ERP.Entity.Auth;
using ERP.Enum;
using ERP.Infrastructure;
using ERP.Repository;
using ERP.Service.Common;

namespace ERP.Service;

/// <summary>
/// This is the ISettingsService interface which inherits all properties of ICrudService and has some methods given below.
/// </summary>
public interface ISettingsService : ICrudService<Setting>
{
    /// <summary>
    /// Retrieves the settings for the tenant identified by the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant whose settings to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{ResultDTO{SettingDTO}}"/> representing the asynchronous operation, 
    /// containing a <see cref="ResultDTO{SettingDTO}"/> with the tenant's settings.
    /// </returns>
    Task<ResultDTO<SettingDTO>> GetSettingByTenantId(string tenantId);

    /// <summary>
    /// Creates a default set of settings for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user for whom to create default settings.</param>
    /// <returns>
    /// A <see cref="Task{Setting}"/> representing the asynchronous operation, 
    /// containing the created <see cref="Setting"/> entity.
    /// </returns>
    Task<Setting> CreateDefaultSettingAsync(User user);
}
/// <summary>
/// Initializes a new instance of the <see cref="SettingService"/> class.
/// </summary>
/// <param name="settingRepository"></param>
/// <param name="unitOfWork"></param>
/// <param name="logger"></param>
public class SettingService(
    ISettingRepository settingRepository,
    IUnitOfWork unitOfWork,
    ILogger<SettingService> logger
    ) : CrudService<Setting>(settingRepository, unitOfWork), ISettingsService
{
    private readonly ISettingRepository _settingRepository = settingRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<SettingService> _logger = logger;

    /// <summary>
    /// Retrieves the settings for the tenant identified by the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant whose settings to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{ResultDTO{SettingDTO}}"/> representing the asynchronous operation, 
    /// containing a <see cref="ResultDTO{SettingDTO}"/> with the tenant's settings.
    /// </returns>
    public async Task<ResultDTO<SettingDTO>> GetSettingByTenantId(string tenantId)
    {
        try
        {
            var setting = await _settingRepository.GetSettingByTenantId(tenantId);
            if (setting == null)
            {
                return ResultDTO<SettingDTO>.Fail("Settings not found for the specified tenant.");

            }
            var result = SettingDTO.MapModelToDTO(setting);
            return ResultDTO<SettingDTO>.Success(result, "Settings retrieved successfully");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings for tenant {TenantId}", tenantId);
            return ResultDTO<SettingDTO>.Fail("An error occurred while retrieving settings");
        }
    }

    /// <summary>
    /// Creates a default set of settings for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user for whom to create default settings.</param>
    /// <returns>
    /// A <see cref="Task{Setting}"/> representing the asynchronous operation, 
    /// containing the created <see cref="Setting"/> entity.
    /// </returns>
    public async Task<Setting> CreateDefaultSettingAsync(User user)
    {
        var setting = new Setting
        {
            Email = user.Email,
            Currency = nameof(Currency.USD),
            ProcessUser = user.Email,
            SelectedAddress = nameof(AddressCategory.Invoice),
            TenantId = user.TenantId,
            Comment = "New SignUp",
            ProductExpiryDays = 30.ToString(),
            IsSupportReq = true,
        };

        await _settingRepository.Add(setting);
        await _unitOfWork.CommitAsync();
        return setting;
    }
}

