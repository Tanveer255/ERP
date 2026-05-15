using ERP.Data.DTO;
using ERP.Data.DTO.Auth;
using ERP.Data.Request;
using ERP.Entity;
using ERP.Enum;
using ERP.Repository;
using ERP.Repository.Auth;
using ERP.Service.Common;
using Microsoft.Extensions.Options;

namespace ERP.Service.Auth;

/// <summary>
/// This is the IAppFileService interface which inherits all properties of ICrudService and has some methods given below.
/// </summary>
public interface IAppFileService : ICrudService<AppFile>
{
    /// <summary>
    /// Method of AppFile Service class to get the user image by user Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<ResultDTO<AppFileDTO>> GetUserImageAsync(Guid id);
    /// <summary>
    /// Method of AppFile Service class to get the file.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<ResultDTO<AppFileDTO>> GetById(Guid id);
    /// <summary>
    /// Method of AppFile Service class to delete the file.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<ResultDTO<bool>> Delete(Guid id);
    /// <summary>
    /// Method of AppFile Service class to get company Logo by company Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<ResultDTO<List<AppFileDTO>>> GetCompanyLogoByCompanyIdAsync(Guid companyId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public AttachmentType GetAttachmentType(string fileName, string contentType);
    /// <summary>
    /// Method of AppFile Service class to get the file by user Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<ResultDTO<List<AppFileDTO>>> GetByUserIdAsync(Guid id);
    /// <summary>
    /// Method of AppFile Service class to upload files list asynchronously.
    /// </summary>
    /// <param name="fileDTOs"></param>
    /// <param name="userId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public Task<ResultDTO<List<AppFileDTO>>> UploadFilesListAsync(List<AppFileDTO> fileDTOs, Guid userId, string tenantId);
    /// <summary>
    /// Method of AppFile Service class to upload a single file asynchronously.
    /// </summary>
    /// <param name="fileDTO"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ResultDTO<AppFileDTO>> UploadSingleFileAsync(AppFileDTO fileDTO, Guid userId, string tenantId);
    /// <summary>
    /// Saves one or more user-uploaded files and associates them with the specified user.
    /// </summary>
    /// <param name="appFiles">A list of files to be uploaded and saved.</param>
    /// <param name="userId">The unique identifier (GUID) of the user to associate the files with.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{List{AppFile}}"/> indicating the outcome and the list of saved files if successful.
    /// </returns>
    Task<ResultDTO<List<AppFile>>> SaveUserFileAsync(List<FormFileRequest> appFiles, Guid userId, string tenantId);
    /// <summary>
    /// Method of AppFile Service class to delete the CompanyLogo by TenantId.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ResultDTO<bool>> DeleteFileIfExistsByTypeAndTenantIdAsync(string attachmentType, string tenantId);
}
/// <summary>
/// Initializes a new instance of the <see cref="AppFileService"/> class.
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="appFileRepository"></param>
/// <param name="logger"></param>
public class AppFileService(
    IUnitOfWork unitOfWork,
    IAppFileRepository appFileRepository,
    ILogger<AppFileService> logger,
    IOptions<FileSettings> fileSettings
    ) : CrudService<AppFile>(appFileRepository, unitOfWork), IAppFileService
{
    private readonly IAppFileRepository _appFileRepository = appFileRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<AppFileService> _logger = logger;
    private readonly FileSettings _fileSettings = fileSettings.Value;
    /// <summary>
    /// Method of AppFile Service class to get the user image by user Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ResultDTO<AppFileDTO>> GetUserImageAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Starting to retrieve file for user with ID: {id}.");
            var file = await _appFileRepository.GetByUserIdAsync(id);
            if (file == null)
            {
                _logger.LogWarning($"No file found for user with ID {id}.");
                return ResultDTO<AppFileDTO>.Fail("File not found.");
            }

            var fileDto = AppFileDTO.MapModelToDto(file);
            return ResultDTO<AppFileDTO>.Success(fileDto, "File retrieved successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(AppFileService)}.{nameof(GetUserImageAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<AppFileDTO>.Fail("Something went wrong. Please try again later.");
        }
    }
    /// <summary>
    /// Method of AppFile Service class to get the file.
    /// </summary>
    /// <param name="id">Paramter to get the id of file.</param>
    /// <returns>AppFile Object after getting.</returns>
    public async Task<ResultDTO<AppFileDTO>> GetById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting by id:" + id);
            var file = await _appFileRepository.GetById(id);
            if (file == null)
            {
                _logger.LogInformation("File not found by id:" + id);
                return ResultDTO<AppFileDTO>.Fail("File not found.");
            }
            _logger.LogInformation("File found by id:" + id);
            var fileDto = AppFileDTO.MapModelToDto(file);
            return ResultDTO<AppFileDTO>.Success(fileDto, "File retrieved successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(AppFileService)}.{nameof(GetById)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<AppFileDTO>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Method of AppFile Service class to delete the file.
    /// </summary>
    /// <param name="id">Paramter to get the id of file.</param>
    /// <returns>AppFile Object after deleting.</returns>
    public async Task<ResultDTO<bool>> Delete(Guid id)
    {
        _logger.LogInformation($"Attempting to retrieve file with ID: {id}");

        var file = await _appFileRepository.GetById(id);
        if (file == null)
        {
            _logger.LogWarning($"File not found for deletion. ID: {id}");
            return ResultDTO<bool>.Fail("File does not exist.");
        }

        _logger.LogInformation($"Deleting file: {file.Name} (ID: {id})");
        await _appFileRepository.Delete(file);
        await _unitOfWork.CommitAsync();

        return ResultDTO<bool>.Success(true, "File deleted successfully.");
    }

    /// <summary>
    ///Method of AppFile Service class to get company Logo by company Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ResultDTO<List<AppFileDTO>>> GetCompanyLogoByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var logos = await _appFileRepository.Get(file =>
                file.CompanyId == companyId &&
                string.Equals(file.Type, nameof(AttachmentType.CompanyLogo), StringComparison.OrdinalIgnoreCase) &&
                !file.IsDeleted);

            var logoList = logos?.ToList() ?? new List<AppFile>();

            if (!logoList.Any())
            {
                return ResultDTO<List<AppFileDTO>>.Fail("No company logo found.");
            }

            var resultDto = AppFileDTO.MapListModelToDto(logoList);
            return ResultDTO<List<AppFileDTO>>.Success(resultDto, "Company logo retrieved successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(AppFileService)}.{nameof(GetCompanyLogoByCompanyIdAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<List<AppFileDTO>>.Fail("Error in retrieving company logo.");
        }
    }
    /// <summary>
    /// Method of AppFile Service class to get attachment type the file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public AttachmentType GetAttachmentType(string fileName, string contentType)
    {
        try
        {
            // Check for image content type and match based on file name
            if (contentType.ToLower().Contains("image"))
            {
                return fileName.ToLower() switch
                {
                    var name when name.Contains("profile") => AttachmentType.ProfileImage,
                    var name when name.Contains("signature") => AttachmentType.UserSignature,
                    var name when name.Contains("avatar") => AttachmentType.Avatar,
                    var name when name.Contains("cover") => AttachmentType.CoverImage,
                    var name when name.Contains("banner") => AttachmentType.BannerImage,
                    var name when name.Contains("thumbnail") => AttachmentType.Thumbnail,
                    _ => AttachmentType.UserImage, // Default to UserImage if not matched
                };
            }

            // Check for document content type
            if (contentType.ToLower().Contains("document"))
            {
                return fileName.ToLower() switch
                {
                    var name when name.Contains("invoice") => AttachmentType.Invoice,
                    var name when name.Contains("contract") => AttachmentType.Contract,
                    var name when name.Contains("receipt") => AttachmentType.Receipt,
                    _ => AttachmentType.Document,  // Default to Document if not matched
                };
            }

            // Check for other file types
            switch (contentType.ToLower())
            {
                case var _ when contentType.Contains("audio"):
                    return AttachmentType.Audio;
                case var _ when contentType.Contains("video"):
                    return AttachmentType.Video;
                default:
                    return AttachmentType.Other;  // Default to Other for unknown file types
            }
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(AppFileService)}.{nameof(GetAttachmentType)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            throw;
        }
    }
    /// <summary>
    /// Method of AppFile Service class to get the file by user Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ResultDTO<List<AppFileDTO>>> GetByUserIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Getting file by id: {id} action: GetByUserIdAsync Controller: AppFileService");
            var files = await _appFileRepository.GetAllByUserIdAsync(id);
            if (files == null)
            {
                _logger.LogInformation($"File not found by id: {id} action: GetByUserIdAsync Controller: AppFileService");
                return ResultDTO<List<AppFileDTO>>.Fail("No files found for the specified user ID.");
            }
            var filesDto = AppFileDTO.MapListModelToDto(files);
            return ResultDTO<List<AppFileDTO>>.Success(filesDto, "Files retrieved successfully for the specified user ID.");
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(AppFileService)}.{nameof(GetByUserIdAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<List<AppFileDTO>>.Fail("Error in Get by user Id ");
        }
    }
    /// <summary>
    /// Method of AppFile Service class to upload files list asynchronously.
    /// </summary>
    /// <param name="fileDTOs"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ResultDTO<List<AppFileDTO>>> UploadFilesListAsync(List<AppFileDTO> fileDTOs, Guid userId, string tenantId)
    {
        if (fileDTOs == null || !fileDTOs.Any())
        {
            return ResultDTO<List<AppFileDTO>>.Fail("No files provided for upload.");
        }

        try
        {
            var files = AppFileDTO.MapListDtoToModel(fileDTOs);
            var validFiles = new List<AppFile>();

            foreach (var file in files)
            {
                if (file == null || string.IsNullOrWhiteSpace(file.Type))
                    continue;

                var fileType = file.Type.ToLowerInvariant();
                if (!_fileSettings.ValidImageContentTypes.Contains(fileType))
                {
                    _logger.LogWarning($"Invalid file type skipped: {file.Name} - {fileType}");
                    continue;
                }

                // Optional: Resize or process large files if needed
                if ((file.Data?.Length ?? 0) / 1000 > 200)
                {
                    // Add resize logic here if required
                    _logger.LogInformation($"File exceeds size limit (200KB), consider resizing: {file.Name}");
                }
                file.UserId = userId;
                file.TenantId = tenantId;
                file.CreatedAt = DateTime.UtcNow;
                file.UpdatedAt = DateTime.UtcNow;

                await _appFileRepository.Add(file);
                validFiles.Add(file);

                _logger.LogInformation($"File prepared for upload: {file.Name}");
            }

            if (!validFiles.Any())
            {
                return ResultDTO<List<AppFileDTO>>.Fail("No valid files were uploaded.");
            }

            await _unitOfWork.CommitAsync();

            var result = AppFileDTO.MapListModelToDto(validFiles);
            return ResultDTO<List<AppFileDTO>>.Success(result, "Files uploaded successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(AppFileService)}.{nameof(UploadFilesListAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<List<AppFileDTO>>.Fail("Error in uploading file ");
        }
    }
    /// <summary>
    /// Method of AppFile Service class to upload a single file asynchronously.
    /// </summary>
    /// <param name="fileDTO"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ResultDTO<AppFileDTO>> UploadSingleFileAsync(AppFileDTO fileDTO, Guid userId, string tenantId)
    {
        if (fileDTO == null || string.IsNullOrWhiteSpace(fileDTO.Type))
        {
            return ResultDTO<AppFileDTO>.Fail("Invalid file data provided.");
        }
        try
        {
            var file = AppFileDTO.MapDtoToModel(fileDTO);
            var fileType = file.Type.ToLowerInvariant();
            if (!_fileSettings.ValidImageContentTypes.Contains(fileType))
            {
                _logger.LogWarning($"Invalid file type: {file.Name} - {fileType}");
                return ResultDTO<AppFileDTO>.Fail("Invalid file type.");
            }
            // Optional: Resize or process large files if needed
            if ((file.Data?.Length ?? 0) / 1000 > 200)
            {
                // Add resize logic here if required
                _logger.LogInformation($"File exceeds size limit (200KB), consider resizing: {file.Name}");
            }
            file.UserId = userId;
            file.TenantId = tenantId;
            file.CreatedAt = DateTime.UtcNow;
            file.UpdatedAt = DateTime.UtcNow;
            await _appFileRepository.Add(file);
            await _unitOfWork.CommitAsync();
            var resultDto = AppFileDTO.MapModelToDto(file);
            return ResultDTO<AppFileDTO>.Success(resultDto, "File uploaded successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(AppFileService)}.{nameof(UploadSingleFileAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<AppFileDTO>.Fail("Error in uploading single file.");
        }
    }
    /// <summary>
    /// Save user file
    /// </summary>
    /// <param name="appFiles"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ResultDTO<List<AppFile>>> SaveUserFileAsync(List<FormFileRequest> appFiles, Guid userId, string tenantId)
    {
        try
        {
            _logger.LogInformation("Start action: SaveUserFileAsync");

            if (appFiles == null || appFiles.Count == 0)
                return ResultDTO<List<AppFile>>.Fail("No files were uploaded.");

            var filesForAdd = new List<AppFile>();
            var filesForUpdate = new List<AppFile>();

            foreach (var file in appFiles)
            {
                if (file.Id != Guid.Empty && file.File is null)
                    continue;

                if (file.Id == Guid.Empty && file.File is null)
                {
                    await DeleteFileIfExistsByTypeAndTenantIdAsync(file.AttachmentType.ToString(), tenantId);
                    continue;
                }

                byte[] fileBytes;
                // Asynchronously read the file bytes
                using (var stream = new MemoryStream())
                {
                    await file.File.CopyToAsync(stream);
                    fileBytes = stream.ToArray();
                }

                string base64String = Convert.ToBase64String(fileBytes);

                var existingFile = await _appFileRepository.GetSingle(
                    x => appFiles.Select(y => y.AttachmentType.ToString()).Contains(x.AttachmentType)
                         && x.TenantId == tenantId
                         && !x.IsDeleted
                         && x.AttachmentType == file.AttachmentType.ToString());

                if (existingFile is not null)
                {
                    existingFile.UpdatedAt = DateTime.UtcNow;
                    existingFile.Data = base64String;
                    existingFile.Type = file.File.ContentType;
                    existingFile.AttachmentFileName = file.File.FileName;
                    filesForUpdate.Add(existingFile);
                }
                else
                {
                    var appFile = new AppFile
                    {
                        Data = base64String,
                        Type = file.File.ContentType,
                        UserId = userId,
                        AttachmentFileName = file.File.FileName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        TenantId = tenantId,
                        AttachmentType = file.AttachmentType.ToString()
                    };
                    filesForAdd.Add(appFile);
                }
            }

            if (filesForAdd.Any())
                await _appFileRepository.Add(filesForAdd);

            if (filesForUpdate.Any())
                await _appFileRepository.Update(filesForUpdate);

            await _unitOfWork.CommitAsync();

            return ResultDTO<List<AppFile>>.Success(filesForAdd, "Files saved successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(SaveUserFileAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<List<AppFile>>.Fail("An error occurred while saving files.");
        }
    }

    public async Task<ResultDTO<bool>> DeleteFileIfExistsByTypeAndTenantIdAsync(string attachmentType, string tenantId)
    {
        _logger.LogInformation($"Attempting to retrieve file with type: {attachmentType} and tenantId: {tenantId}");

        var file = await _appFileRepository.GetByTypeAndTenantIdAsync(attachmentType, tenantId);
        if (file == null)
        {
            _logger.LogWarning($"File not found for deletion. tenantId: {tenantId} and type: {attachmentType}");
            return ResultDTO<bool>.Fail("File does not exist.");
        }

        _logger.LogInformation($"Deleting file: {file.Name} (tenantId: {tenantId} and type: {attachmentType})");
        await _appFileRepository.Delete(file);
        await _unitOfWork.CommitAsync();

        return ResultDTO<bool>.Success(true, "File deleted successfully.");
    }
}
