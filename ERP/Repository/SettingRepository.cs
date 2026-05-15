using ERP.Entity;

namespace ERP.Repository;

public interface ISettingRepository : IRepository<Setting>
{
    Task<Setting> GetSettingByTenantId(string tenantId);
    public Task<Guid> GetSettingIdByTenantId(string tenantId);
    Task<bool> IsSupportEnabled(string tenantId);
}
public class SettingRepository(
    ILogger<SettingRepository> logger,
    IUnitOfWork unitOfWork
    ) : Repository<Setting>(unitOfWork, logger), ISettingRepository
{
    private readonly ILogger<SettingRepository> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<Setting> GetSettingByTenantId(string tenantId)
    {
        _logger.LogInformation("GetSettingByTenantId called with tenantId: {tenantId}", tenantId);
        return await GetSingle(x => x.TenantId == tenantId);
    }
    public async Task<Guid> GetSettingIdByTenantId(string tenantId)
    {
        _logger.LogInformation("GetSettingByTenantId called with tenantId: {tenantId}", tenantId);
        return await _unitOfWork.Context.Settings.Where(x => x.TenantId.Equals(tenantId)).Select(x => x.Id).FirstOrDefaultAsync();
    }

    public async Task<bool> IsSupportEnabled(string tenantId)
    {
        return await GetSingle(x => x.TenantId == tenantId).Select(x => x.IsSupportReq);
    }
}

