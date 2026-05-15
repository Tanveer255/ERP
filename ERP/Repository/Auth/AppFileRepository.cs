using ERP.Entity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Auth;

public interface IAppFileRepository : IRepository<AppFile>
{
    public Task<AppFile> GetById(Guid id);
    Task<AppFile> GetByUserIdAsync(Guid userId);
    Task<List<AppFile>> GetAllByUserIdAsync(Guid userId);
    Task<AppFile> GetByType(string imageType, Guid userId);
    Task<AppFile> GetByTypeAndTenantIdAsync(string imageType, string tenantId);
}
public class AppFileRepository(
    IUnitOfWork unitOfWork,
    ILogger<AppFileRepository> logger
    ) : Repository<AppFile>(unitOfWork, logger), IAppFileRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<AppFileRepository> _logger = logger;
    public async Task<AppFile> GetById(Guid id)
    {
        _logger.LogInformation($"Getting file by id: {id} action: GetById Controller: AppFileRepository");
        var file = await _unitOfWork.Context.AppFiles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (file == null)
        {
            _logger.LogInformation($"File not found by id: {id} action: GetById Controller: AppFileRepository");
            return null;
        }
        return file;
    }
    public async Task<AppFile> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation($"Getting file by id: {userId} action: GetById Controller: AppFileRepository");
        var file = await _unitOfWork.Context.AppFiles.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
        if (file == null)
        {
            _logger.LogInformation($"File not found by id: {userId} action: GetById Controller: AppFileRepository");
            return null;
        }
        return file;
    }

    public async Task<List<AppFile>> GetAllByUserIdAsync(Guid userId)
    {
        _logger.LogInformation($"Getting file by id: {userId} action: GetById Controller: AppFileRepository");
        var files = await _unitOfWork.Context.AppFiles.Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync();
        if (files is null)
        {
            _logger.LogInformation($"File not found by id: {userId} action: GetById Controller: AppFileRepository");
            return null;
        }
        return files;
    }
    public async Task<AppFile> GetByType(string imageType, Guid userId)
    {
        _logger.LogInformation($"Getting file by type: {imageType} action: GetByType Controller: AppFileRepository");
        var file = await _unitOfWork.Context.AppFiles.FirstOrDefaultAsync(x => x.AttachmentType == imageType && x.UserId == userId && !x.IsDeleted);
        if (file == null)
        {
            _logger.LogInformation($"File not found by type: {imageType} action: GetByType Controller: AppFileRepository");
            return null;
        }
        return file;
    }

    public async Task<AppFile> GetByTypeAndTenantIdAsync(string imageType, string tenantId)
    {
        _logger.LogInformation($"Getting file by type: {imageType} and tenantId: {tenantId} action: GetByTypeAndTenantIdAsync Controller: AppFileRepository");
        var file = await GetAllReadOnly()
                            .FirstOrDefaultAsync(x => x.AttachmentType == imageType && x.TenantId == tenantId && !x.IsDeleted);
        if (file == null)
        {
            _logger.LogInformation($"File not found by type: {imageType} action: GetByType Controller: AppFileRepository");
            return null;
        }
        return file;
    }
}
