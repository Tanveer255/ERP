using ERP.Data.DTO;
using ERP.Data.DTO.Auth;
using ERP.Data.Request;
using ERP.Entity.Auth;
using ERP.Enum;
using ERP.Infrastructure;
using ERP.Repository;
using ERP.Repository.Auth;
using ERP.Service.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ERP.Service.Auth;

/// <summary>
/// This is the ICompanyService interface which inherits all properties of ICrudService and has some methods given below.
/// </summary>
public interface ICompanyService : ICrudService<Company>
{
    /// <summary>
    /// Updates the company information based on the provided <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The company data to be updated.</param>
    /// <returns>
    /// A <see cref="ResultDTO{Boolean}"/> indicating whether the update was successful.
    /// </returns>
    Task<ResultDTO<bool>> UpdateAsync(CompanyDTO request);

    /// <summary>
    /// Updates the list of addresses associated with the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="addressTypeDTOs">The list of address data to be updated.</param>
    /// <param name="tenantId">The tenant identifier for which the addresses belong.</param>
    /// <returns>
    /// A <see cref="ResultDTO{Boolean}"/> indicating whether the update operation was successful.
    /// </returns>
    Task<ResultDTO<bool>> UpdateAddressesAsync(List<AddressTypeDTO> addressTypeDTOs, string tenantId);

    /// <summary>
    /// Retrieves the company details for the specified <paramref name="tenantId"/>, 
    /// optionally scoped or filtered by the provided <paramref name="userId"/>.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <param name="userId">The user identifier for context or access control.</param>
    /// <returns>
    /// A <see cref="ResultDTO{CompanyDTO}"/> containing the company information, if found.
    /// </returns>
    Task<ResultDTO<CompanyDTO>> GetByTenantIdAsync(string tenantId, Guid userId);

    /// <summary>
    /// Creates a new company entity based on the provided <paramref name="request"/>, 
    /// associated with the specified <paramref name="tenantId"/> and initiated by the provided <paramref name="processUser"/>.
    /// </summary>
    /// <param name="request">The sign-up request containing company creation details.</param>
    /// <param name="tenantId">The unique identifier of the tenant to associate with the company.</param>
    /// <param name="processUser">The identifier of the user initiating the creation process.</param>
    /// <returns>
    /// A <see cref="Task{Company}"/> representing the asynchronous operation, containing the created <see cref="Company"/> entity.
    /// </returns>
    Task<Company> CreateCompanyAsync(SignUpRequest request, string tenantId, string processUser);
}
/// <summary>
/// Initializes a new instance of the <see cref="CompanyService"/> class.
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="companyRepository"></param>
/// <param name="addressTypeRepository"></param>
/// <param name="appFileRepository"></param>
public class CompanyService(
    IUnitOfWork unitOfWork,
    ICompanyRepository companyRepository,
    IAddressTypeRepository addressTypeRepository,
    IAppFileRepository appFileRepository,
    IAppFileService appFileService,
    ILogger<CompanyService> logger

    ) : CrudService<Company>(companyRepository, unitOfWork), ICompanyService
{
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAddressTypeRepository _addressTypeRepository = addressTypeRepository;
    private readonly IAppFileRepository _appFileRepository = appFileRepository;
    private readonly IAppFileService _appFileService = appFileService;
    private readonly ILogger<CompanyService> _logger = logger;

    /// <summary>
    /// Updates the company information based on the provided <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The company data to be updated.</param>
    /// <returns>
    /// A <see cref="ResultDTO{Boolean}"/> indicating whether the update was successful.
    /// </returns>
    public async Task<ResultDTO<bool>> UpdateAsync(CompanyDTO request)
    {
        try
        {
            var isCompanyExist = await _companyRepository.IsExist(request.Id, request.TenantId);
            if (!isCompanyExist)
            {
                return ResultDTO<bool>.Fail("Company does not exist.");
            }

            var company = request.MapDtoToModel(request);
            company.UpdatedAt = DateTime.UtcNow;
            company.IsPartner = true;
            company.PartnerType = "FedEx";
            await _companyRepository.Update(company);

            if (request.FormFile is not null)
            {
                await _appFileService.SaveUserFileAsync(new List<FormFileRequest> { request.FormFile }, request.UserId, request.TenantId);
            }

            await _unitOfWork.CommitAsync();
            return ResultDTO<bool>.Success(true, "Company was saved successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(CompanyService)}.{nameof(GetByTenantIdAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<bool>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Updates the list of addresses associated with the specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="addressTypeDTOs">The list of address data to be updated.</param>
    /// <param name="tenantId">The tenant identifier for which the addresses belong.</param>
    /// <returns>
    /// A <see cref="ResultDTO{Boolean}"/> indicating whether the update operation was successful.
    /// </returns>

    public async Task<ResultDTO<bool>> UpdateAddressesAsync(List<AddressTypeDTO> addressTypeDTOs, string tenantId)
    {
        try
        {
            var companyDTO = await _companyRepository.GetByTenantIdAsync(tenantId);
            if (companyDTO is null)
            {
                return ResultDTO<bool>.Fail("Company does not exist.");
            }

            var existingAddressTypes = await _addressTypeRepository.GetByCompanyIdAsync(companyDTO.Id, tenantId);

            foreach (var addressTypeDTO in addressTypeDTOs)
            {
                var result = existingAddressTypes.FirstOrDefault(f => f.Id.Equals(addressTypeDTO.Id));
                if (result is null)
                {
                    return ResultDTO<bool>.Fail("Your are providing invalid address Id.");
                }

                result.AddressLine = addressTypeDTO.AddressLine;
                result.AddressLine2 = addressTypeDTO.AddressLine2;
                result.TownLocality = addressTypeDTO.TownLocality;
                result.CityRegion = addressTypeDTO.CityRegion;
                result.State = addressTypeDTO.State;
                result.PostalZipCode = addressTypeDTO.PostalZipCode;
                result.CountryId = addressTypeDTO.CountryId;
                result.CountryName = addressTypeDTO.CountryName;
                result.Type = addressTypeDTO.Type;
                result.PhoneCountryCode = addressTypeDTO.PhoneCountryCode;
                result.PhoneNo = addressTypeDTO.PhoneNo;
                result.UpdatedAt = DateTime.UtcNow;
                await _addressTypeRepository.Update(result);
            }

            await _unitOfWork.CommitAsync();
            return ResultDTO<bool>.Success(true, "Addresses have been saved successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(CompanyService)}.{nameof(GetByTenantIdAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<bool>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Retrieves the company details for the specified <paramref name="tenantId"/>, 
    /// optionally scoped or filtered by the provided <paramref name="userId"/>.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <param name="userId">The user identifier for context or access control.</param>
    /// <returns>
    /// A <see cref="ResultDTO{CompanyDTO}"/> containing the company information, if found.
    /// </returns>
    public async Task<ResultDTO<CompanyDTO>> GetByTenantIdAsync(string tenantId, Guid userId)
    {
        try
        {
            var company = await _companyRepository.GetByTenantIdAsync(tenantId);
            if (company is null)
            {
                return ResultDTO<CompanyDTO>.Fail("Company does not exist.");
            }

            company.CompanyLogo = await _appFileRepository.GetByType(nameof(AttachmentType.CompanyLogo), userId);
            company.AddressTypes = (await _addressTypeRepository.GetByCompanyIdAsync(company.Id, tenantId))
                                            .Select(address => new AddressTypeDTO().MapModelToDto(address))
                                            .ToList();

            return ResultDTO<CompanyDTO>.Success(company, "Your requested data has been retrieved successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(CompanyService)}.{nameof(GetByTenantIdAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<CompanyDTO>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Creates a new company entity based on the provided <paramref name="request"/>, 
    /// associated with the specified <paramref name="tenantId"/> and initiated by the provided <paramref name="processUser"/>.
    /// </summary>
    /// <param name="request">The sign-up request containing company creation details.</param>
    /// <param name="tenantId">The unique identifier of the tenant to associate with the company.</param>
    /// <param name="processUser">The identifier of the user initiating the creation process.</param>
    /// <returns>
    /// A <see cref="Task{Company}"/> representing the asynchronous operation, containing the created <see cref="Company"/> entity.
    /// </returns>
    public async Task<Company> CreateCompanyAsync(SignUpRequest request, string tenantId, string processUser)
    {
        var company = new Company
        {
            TenantId = tenantId,
            CompanyName = string.IsNullOrEmpty(request.BusinessName) ? string.Empty : request.BusinessName,
            ProcessUser = string.IsNullOrEmpty(processUser) ? string.Empty : processUser,
            TaxIDorVATNo = "TBC",
            TurnoverCcy = nameof(Currency.USD),
            IsNewSignUp = true,
            IsPartner = true,
            //PartnerType = nameof(PartnerType.FedEx)
        };

        await _companyRepository.Add(company);
        await _unitOfWork.CommitAsync();
        return company;
    }
}
