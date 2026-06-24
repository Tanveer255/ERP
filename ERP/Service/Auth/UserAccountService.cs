using DnsClient;
using ERP.Data.DTO;
using ERP.Data.DTO.Auth;
using ERP.Data.Request;
using ERP.Entity;
using ERP.Entity.Auth;
using ERP.Enum;
using ERP.Repository;
using ERP.Repository.Auth;
using ERP.Service.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net;

namespace ERP.Service.Auth;

/// <summary>
/// This is the IUserAccountService interface which inherits all properties of ICrudService and has some methods given below.
/// </summary>
public interface IUserAccountService : ICrudService<User>
{
    /// <summary>
    /// Login user with varify user Email, password and also generate token with user claims
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ResultDTO<LoginResponce>> LoginAsync(LogInRequest request);

    /// <summary>
    /// Logs out the user by invalidating the provided refresh token or session data.
    /// </summary>
    /// <param name="request">The request containing logout details, such as the refresh token or device information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{LogoutResponce}"/> indicating the outcome of the logout operation.
    /// </returns>
    Task<ResultDTO<LogoutResponce>> LogoutAsync(LogOutRequest request);

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier (GUID) of the user to be deleted.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{Boolean}"/> indicating whether the deletion was successful.
    /// </returns>
    Task<ResultDTO<bool>> DeleteUserAsync(Guid userId);

    /// <summary>
    /// It will delete user parmanently in case of exception during signup
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> DiscardUserAsync(Guid userId);

    /// <summary>
    /// Updates the profile information of the specified user.
    /// </summary>
    /// <param name="request">The request containing updated profile data such as name, contact info, or profile image.</param>
    /// <param name="userId">The unique identifier (GUID) of the user whose profile is being updated.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{UpdateUserResponse}"/> indicating the outcome and the updated user profile data if successful.
    /// </returns>
    Task<ResultDTO<UpdateUserResponse>> UpdateProfileAsync(UpdateUserRequest request, Guid userId);

    /// <summary>
    /// Retrieves the profile information of the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier (GUID) of the user whose profile is to be retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{GetProfileResponse}"/> with the user's profile data if found.
    /// </returns>
    Task<ResultDTO<GetProfileResponse>> GetProfileAsync(Guid userId);

    /// <summary>
    /// Registers a new user using the provided sign-up information.
    /// </summary>
    /// <param name="request">The request containing user registration details such as name, email, and password.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{Boolean}"/> indicating whether the sign-up was successful.
    /// </returns>
    Task<ResultDTO<bool>> SignupAsync(SignUpRequest request);

    /// <summary>
    /// Confirms a user's email address using the provided verification token.
    /// </summary>
    /// <param name="request">The request containing the email confirmation token and associated user information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{LoginResponce}"/> indicating whether the confirmation was successful and may include login-related data.
    /// </returns>
    Task<ResultDTO<LoginResponce>> ConfirmEmailAsync(ValidateTokenRequest request);

    /// <summary>
    /// Changes the password of the specified user after validating the current password.
    /// </summary>
    /// <param name="request">The request containing the current and new password.</param>
    /// <param name="userId">The unique identifier (GUID) of the user changing the password.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{ChangePasswordRequest}"/> indicating whether the password change was successful.
    /// </returns>
    Task<ResultDTO<ChangePasswordRequest>> ChangePasswordAsync(ChangePasswordRequest request, Guid userId);

    /// <summary>
    /// Sends a password reset email to the user with a reset token.
    /// </summary>
    /// <param name="request">The request containing the user's email address.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{ForgotPasswordEmailRequest}"/> indicating whether the email was sent successfully.
    /// </returns>
    Task<ResultDTO<ForgotPasswordEmailRequest>> ForgotPasswordEmail(ForgotPasswordEmailRequest request);

    /// <summary>
    /// Resets the user's password using the provided reset token and new password.
    /// </summary>
    /// <param name="response">The request containing the reset token, email, and new password.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ResultDTO{Boolean}"/> indicating whether the operation succeeded.
    /// </returns>
    Task<ResultDTO<bool>> ResetPasswordAsync(ERP.Data.Request.ResetPasswordRequest response);

    /// <summary>
    /// // Resend email confirmation for user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ResultDTO<bool>> ResendEmailAsync(ResendEmailConfirmation request);

    /// <summary>
    /// Method of UserAccount Service to get login response data.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<LoginResponce> GetLoginResponseData(User user);

    /// <summary>
    /// Toggles the Support Request setting for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result with the toggle success status.</returns>
    Task<ResultDTO<bool>> ToggleSupportRequest(string tenantId);

    /// <summary>
    /// Get the Support Request setting for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result with the toggle success status.</returns>
    Task<ResultDTO<GetSupportReqResponse>> GetSupportRequest(string tenantId);
}

/// <summary>
/// Initializes a new instance of the <see cref="UserAccountService"/> class.
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="userRepository"></param>
/// <param name="jwtAuthentication"></param>
/// <param name="emailService"></param>
/// <param name="jwtSettings"></param>
/// <param name="logger"></param>
/// <param name="userManager"></param>
/// <param name="appFileService"></param>
/// <param name="tenantService"></param>
/// <param name="tenantRepository"></param>
/// <param name="companyService"></param>
/// <param name="addressTypeRepository"></param>
/// <param name="publishEndpoint"></param>
/// <param name="RefreshTokenService"></param>
/// <param name="settingRepository"></param>
/// <param name="subscriptionRepository"></param>
/// <param name="recaptchaService"></param>
public class UserAccountService(
    IUnitOfWork unitOfWork,
    IUserAccountRepository userRepository,
    IJwtAuthenticationService jwtAuthentication,
    IEmailService emailService,
    IOptions<JwtSettings> jwtSettings,
    ILogger<UserAccountService> logger,
    UserManager<User> userManager,
    IAppFileService appFileService,
    ITenantService tenantService,
    ICompanyService companyService,
    ISettingRepository settingRepository,
    IRecaptchaService recaptchaService,
    IHttpContextAccessor httpContextAccessor,
    IPasswordHasher<User> passwordHasher,
    IAddressTypeService addressTypeService,
    ISettingsService settingsService,
    IPasswordValidator passwordValidator,
    IMemoryCache cache,
    IWebHostEnvironment environment
    ) : CrudService<User>(userRepository, unitOfWork), IUserAccountService
{
    private readonly IUserAccountRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly IJwtAuthenticationService _jwtAuthentication = jwtAuthentication;
    private readonly ILogger<UserAccountService> _logger = logger;
    private readonly IAppFileService _appFileService = appFileService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly ICompanyService _companyService = companyService;
    private readonly IAddressTypeService _addressTypeService = addressTypeService;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ISettingRepository _settingRepository = settingRepository;
    private readonly IRecaptchaService _recaptchaService = recaptchaService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly ISettingsService _settingsService = settingsService;
    private readonly IPasswordValidator _passwordValidator = passwordValidator;
    private readonly IMemoryCache _cache = cache;
    private readonly IWebHostEnvironment _environment = environment;

    /// <summary>
    /// Login user with varify user Email, password and also generate token with user claims
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ResultDTO<LoginResponce>> LoginAsync(LogInRequest request)
    {
        try
        {
            var captchaValid = await _recaptchaService.VerifyAsync(request.RecaptchaToken);
            if (!captchaValid)
                return ResultDTO<LoginResponce>.Fail("Captcha verification failed.");

            var user = await _userRepository.GetRegularUserByEmailAsync(request.Email);
            if (user is null)
                return ResultDTO<LoginResponce>.Fail("Invalid email or password.");

            if (!_passwordValidator.VerifyPassword(user, request.Password))
                return ResultDTO<LoginResponce>.Fail("Invalid email or password.");

            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var signUpEmailDTO = new SignUpEmailDTO()
                {
                    Email = request.Email,
                    Token = token,
                    FirstName = user.FirstName,
                };
                await _emailService.SendSignUpEmail(signUpEmailDTO);
                return ResultDTO<LoginResponce>.Fail("The email address has not been confirmed. A new verification email has been sent.");
            }

            var response = await GetLoginResponseData(user);

            user.LastActivity = DateTime.UtcNow;
            await _userRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return ResultDTO<LoginResponce>.Success(response, "You have successfully logged in.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(LoginAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<LoginResponce>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Method of UserAccount Service to get login response data.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<LoginResponce> GetLoginResponseData(User user)
    {
        var setting = await _settingRepository.GetSettingByTenantId(user.TenantId);
        var roles = await _userManager.GetRolesAsync(user);
        var generateToken = new GenerateTokenRequest
        {
            Email = user.Email,
            TenantId = user.TenantId,
            UserId = user.Id,
            SettingId = setting.Id,
            Role = roles.FirstOrDefault() ?? nameof(AccessRole.Staff)
        };

        var token = await _jwtAuthentication.GenerateTokenAsync(generateToken);
        var cookies = _httpContextAccessor.HttpContext.Response.Cookies;
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/",
        };

        cookies.Append("AuthToken", token, options);

        var files = await _appFileService.GetByUserIdAsync(user.Id);

        LoginResponce responce = new()
        {
            Email = user.Email,
            TenantId = user.TenantId,
            FullName = $"{user.FirstName} {user.LastName}",
            FormFiles = files.Data
        };

        return responce;
    }

    /// <summary>
    /// Method of UserAccount Service to Logout user and remove token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ResultDTO<LogoutResponce>> LogoutAsync(LogOutRequest request)
    {
        try
        {
            var user = await _userRepository.GetRegularUserByEmailAsync(request.Email);
            if (user == null)
                return ResultDTO<LogoutResponce>.Fail("Invalid user email.");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Path = "/"
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Delete("AuthToken", cookieOptions);

            LogoutResponce responce = new()
            {
                Token = null,
            };

            return ResultDTO<LogoutResponce>.Success(responce, "You’ve logged out successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(LogoutAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<LogoutResponce>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// delete user by id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ResultDTO<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return ResultDTO<bool>.Fail("User not found.");

            await _userRepository.Delete(user);
            await _unitOfWork.CommitAsync();
            return ResultDTO<bool>.Success(true, "User deleted successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(DeleteUserAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<bool>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// It will delete user parmanently in case of exception during signup
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> DiscardUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                await _userRepository.Discard(user);
                await _unitOfWork.CommitAsync();
                return true;
            }
            return false;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(DiscardUserAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return false;
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ResultDTO<UpdateUserResponse>> UpdateProfileAsync(UpdateUserRequest request, Guid userId)
    {
        try
        {
            var existUser = await _userRepository.GetUserByIdAsync(userId);
            if (existUser == null)
                return ResultDTO<UpdateUserResponse>.Fail("User not found.");

            existUser.FirstName = request.FirstName;
            existUser.LastName = request.LastName;
            existUser.PhoneNumber = request.PhoneNumber;
            existUser.CountryCode = request.CountryCode;
            await _userRepository.Update(existUser);
            var result = await _appFileService.SaveUserFileAsync(request.FormFiles, existUser.Id, existUser.TenantId);
            await _unitOfWork.CommitAsync();
            var response = new UpdateUserResponse
            {
                FirstName = existUser.FirstName,
                LastName = existUser.LastName,
                Email = existUser.Email,
                PhoneNumber = existUser.PhoneNumber,
            };
            return ResultDTO<UpdateUserResponse>.Success(response, "Profile updated successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(UpdateProfileAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<UpdateUserResponse>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Signup confirmation and also created company, setting, default setting, and Subscription for new User
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ResultDTO<bool>> SignupAsync(SignUpRequest request)
    {
        try
        {
            // Step 1: Validate
            if (!await _recaptchaService.VerifyAsync(request.RecaptchaToken))
                return ResultDTO<bool>.Fail("Captcha verification failed.");

            if (await _userRepository.IsUserExistByEmailAsync(request.Email))
                return ResultDTO<bool>.Fail("An account with this email already exists. Please sign in or reset your password.");

            if (!await ValidateDomain(request.Email))
                return ResultDTO<bool>.Fail("Please enter a valid email address.");

            // Step 2: Create tenant
            var tenant = await _tenantService.CreateTenant(request.BusinessName);

            // Step 3: Create user
            var user = new User
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                CountryCode = request.CountryCode,
                SecurityStamp = Guid.NewGuid().ToString(),
                TenantId = tenant.TenantId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email.ToUpper(),
                Status = nameof(UserStatus.Active),
                UserType = nameof(AccessRole.Registrar),
                IsTermsAgreed = request.IsTermsAgreed,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _userRepository.Add(user);
            await _unitOfWork.CommitAsync();

            // Step 4: Create company
            var company = await _companyService.CreateCompanyAsync(request, tenant.TenantId, user.Email);

            // Step 5: Create addresses
            await _addressTypeService.CreateDefaultAddressesAsync(company.Id, tenant.TenantId);

            // Step 6: Create settings
            var setting = await _settingsService.CreateDefaultSettingAsync(user);

            // Step 7: Send signup email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            bool validEmail = await _emailService.ValidateEmail(request.Email);
            if (validEmail)
            {
                var signUpEmailDTO = new SignUpEmailDTO()
                {
                    Email = request.Email,
                    Token = token,
                    FirstName = user.FirstName,
                };
                await _emailService.SendSignUpEmail(signUpEmailDTO);
                await _emailService.SupportSignupAlertTemplate(request.Email);
            }
            return ResultDTO<bool>.Success(true, "Verification email sent. Please check your inbox.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(SignupAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<bool>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Checks if the email domain is capable of receiving emails (i.e., has MX records).
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>True if the domain can receive emails, false otherwise.</returns>
    public async Task<bool> ValidateDomain(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return false;

        try
        {
            var domain = email.Split('@')[1].Trim();
            var options = new LookupClientOptions(IPAddress.Parse("8.8.8.8"))
            {
                Timeout = TimeSpan.FromMilliseconds(500),
                Retries = 1,
                UseTcpFallback = false,
                UseCache = true
            };

            var lookup = new LookupClient(options);

            var result = await lookup.QueryAsync(domain, QueryType.MX);
            return result.Answers.MxRecords().Any();
        }
        catch (DnsResponseException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get Profile Async
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ResultDTO<GetProfileResponse>> GetProfileAsync(Guid userId)
    {
        try
        {
            var existUser = await _userRepository.GetUserByIdAsync(userId);
            if (existUser == null)
                return ResultDTO<GetProfileResponse>.Fail("User not found.");

            var files = await _appFileService.GetByUserIdAsync(userId);
            var response = new GetProfileResponse
            {
                FirstName = existUser.FirstName,
                LastName = existUser.LastName,
                Email = existUser.Email,
                PhoneNumber = existUser.PhoneNumber,
                CountryCode = existUser.CountryCode,
                FormFiles = files.Data
            };
            return ResultDTO<GetProfileResponse>.Success(response, "Profile updated successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(GetProfileAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<GetProfileResponse>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Method of User Account Service to confirm email.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ResultDTO<LoginResponce>> ConfirmEmailAsync(ValidateTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token))
                return ResultDTO<LoginResponce>.Fail("Invalid token or email.");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return ResultDTO<LoginResponce>.Fail("User not found.");

            if (user.EmailConfirmed)
                return ResultDTO<LoginResponce>.Fail("Your email is already confirmed.");

            var decodedToken = WebUtility.UrlDecode(request.Token).Replace(" ", "+");
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                var errorDetails = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.BeginScope($"SignUp token confirmation failed for user {user.Email}: {errorDetails}");
                return ResultDTO<LoginResponce>.Fail($"Something went wrong. Please try again later.");
            }
            user.EmailConfirmed = true;
            await _userRepository.Update(user);

            var response = await GetLoginResponseData(user);
            return ResultDTO<LoginResponce>.Success(response, "Email confirmed successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(ConfirmEmailAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<LoginResponce>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Change password for user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ResultDTO<ChangePasswordRequest>> ChangePasswordAsync(ChangePasswordRequest request, Guid userId)
    {
        try
        {
            if (request.CurrentPassword == request.NewPassword)
                return ResultDTO<ChangePasswordRequest>.Fail("New password must be different from the current password.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return ResultDTO<ChangePasswordRequest>.Fail("User not found in token.");

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
                return ResultDTO<ChangePasswordRequest>.Fail("Incorrect current password.");

            _logger.LogInformation($"User {user} changed their password successfully.");
            return ResultDTO<ChangePasswordRequest>.Success(request, "Your password has been reset successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(ChangePasswordAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<ChangePasswordRequest>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Method of User Account Service to send reset password email.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ResultDTO<ForgotPasswordEmailRequest>> ForgotPasswordEmail(ForgotPasswordEmailRequest request)
    {
        try
        {
            if (!await _recaptchaService.VerifyAsync(request.RecaptchaToken))
                return ResultDTO<ForgotPasswordEmailRequest>.Fail("Captcha verification failed.");
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return ResultDTO<ForgotPasswordEmailRequest>.Fail("Invalid user email.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            bool validEmail = await _emailService.ValidateEmail(request.Email);
            if (validEmail)
            {
                await _emailService.SendForgotPasswordEmail(request.Email, token, user.FirstName);
                await _userManager.UpdateAsync(user);
            }

            return ResultDTO<ForgotPasswordEmailRequest>.Success(request, "Your email has been sent successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(ForgotPasswordEmail)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<ForgotPasswordEmailRequest>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Method of User Account Service to reset password.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ResultDTO<bool>> ResetPasswordAsync(ERP.Data.Request.ResetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || user.UserType == nameof(AccessRole.Staff))
                return ResultDTO<bool>.Fail("Invalid user email.");

            if (_passwordValidator.VerifyPassword(user, request.Password))
                return ResultDTO<bool>.Fail("New password must be different from the current password.");

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (!result.Succeeded)
                return ResultDTO<bool>.Fail("This reset link has already been used.");

            _logger.LogInformation($"User {user} reset their password successfully.");
            return ResultDTO<bool>.Success(true, "Password updated succesfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(ResetPasswordAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<bool>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Method of User Account Service to resend email confirmation link.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ResultDTO<bool>> ResendEmailAsync(ResendEmailConfirmation request)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return ResultDTO<bool>.Fail("User not found.");

            if (user.EmailConfirmed)
                return ResultDTO<bool>.Success(true, "Email is already confirmed.");

            if (_cache.TryGetValue<DateTime>($"resend:{user.Email}", out var lastSentAt))
            {
                var waitTime = lastSentAt.AddMinutes(1) - DateTime.UtcNow;
                if (waitTime.TotalSeconds > 0)
                {
                    return ResultDTO<bool>.Fail(
                        $"Please wait {waitTime.Seconds} seconds before requesting another email."
                    );
                }
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var isEmailValid = await _emailService.ValidateEmail(request.Email);
            if (!isEmailValid)
                return ResultDTO<bool>.Fail("Invalid email address.");

            var emailDto = new SignUpEmailDTO
            {
                Email = user.Email,
                Token = token,
                FirstName = user.FirstName
            };

            await _emailService.SendSignUpEmail(emailDto);
            _cache.Set($"resend:{user.Email}", DateTime.UtcNow, TimeSpan.FromMinutes(1));
            return ResultDTO<bool>.Success(true, "Verification email sent. Please check your inbox.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(UserAccountService)}.{nameof(ResendEmailAsync)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<bool>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Toggles the Support Request setting for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result with the toggle success status.</returns>
    public async Task<ResultDTO<bool>> ToggleSupportRequest(string tenantId)
    {
        try
        {
            var user = await _userRepository.GetUserByTenantIdAsync(tenantId);
            if (user is null)
                return ResultDTO<bool>.Fail("No user found for the provided tenant ID.");

            var setting = await _settingRepository.GetSingle(x => x.TenantId == tenantId);
            if (setting is null)
                return ResultDTO<bool>.Fail("No check found for the provided tenant ID.");

            setting.IsSupportReq = !setting.IsSupportReq;
            setting.UpdatedAt = DateTime.UtcNow;

            if (setting.IsSupportReq)
            {
                var userDetails = await _userRepository.GetUserDetailsForEmailByTenantIdAsync(tenantId);
                await _emailService.NotifySupportRequested(userDetails);
            }

            await _settingRepository.Update(setting);
            await _unitOfWork.CommitAsync();
            return ResultDTO<bool>.Success(true, "Support request updated successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(SettingService)}.{nameof(ToggleSupportRequest)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<bool>.Fail("Something went wrong. Please try again later.");
        }
    }

    /// <summary>
    /// Get the Support Request setting for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result with the toggle success status.</returns>
    public async Task<ResultDTO<GetSupportReqResponse>> GetSupportRequest(string tenantId)
    {
        try
        {
            var setting = await _settingRepository.GetSingle(x => x.TenantId == tenantId);
            if (setting is null)
            {
                return ResultDTO<GetSupportReqResponse>.Fail("No check found for the provided tenant ID.");
            }

            var result = new GetSupportReqResponse { IsSupportReq = setting.IsSupportReq };
            return ResultDTO<GetSupportReqResponse>.Success(result, "Support request retrieved successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in {nameof(SettingService)}.{nameof(ToggleSupportRequest)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return ResultDTO<GetSupportReqResponse>.Fail("Something went wrong. Please try again later.");
        }
    }
}
