using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.Auth;

public class UsersController(IUserAccountService userService, IServicePriceService servicePriceService) : ApiBaseController
{
    private readonly IUserAccountService _userService = userService;
    private readonly IServicePriceService _servicePriceService = servicePriceService;

    /// <summary>
    /// Authenticates a user and returns a token if successful.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>
    /// Returns 200 OK with user details and token if authentication is successful; 
    /// otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [HttpPost(nameof(Login))]
    public async Task<IActionResult> Login([FromBody] LogInRequest request)
    {
        var result = await _userService.LoginAsync(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Logout a user and returns a null token if successful.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost(nameof(Logout))]
    public async Task<IActionResult> Logout([FromBody] LogOutRequest request)
    {
        var result = await _userService.LogoutAsync(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    ///  Signup confirmation and also created company, setting, default setting, and Subscription If User Exist
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost(nameof(Signup))]
    public async Task<IActionResult> Signup([FromBody] SignUpRequest request)
    {
        var result = await _userService.SignupAsync(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Confirms a user's email address using the provided verification token.
    /// </summary>
    /// <param name="request">The token request containing the email confirmation token.</param>
    /// <returns>
    /// Returns 200 OK if the email was successfully confirmed; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [HttpPost(nameof(ConfirmEmail))]
    public async Task<IActionResult> ConfirmEmail([FromBody] ValidateTokenRequest request)
    {
        var result = await _userService.ConfirmEmailAsync(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    /// <param name="model">The request model containing the current and new password.</param>
    /// <returns>
    /// Returns 200 OK if the password was changed successfully; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [Authorize, HttpPost(nameof(ChangePassword))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
    {
        var result = await _userService.ChangePasswordAsync(model, User.GetUserId());
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Sends a password reset email to the user with a reset token.
    /// </summary>
    /// <param name="request">The request containing the user's email address.</param>
    /// <returns>
    /// Returns 200 OK if the reset email was sent successfully; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [HttpPost(nameof(ForgotPasswordEmail))]
    public async Task<IActionResult> ForgotPasswordEmail([FromBody] ForgotPasswordEmailRequest request)
    {
        var result = await _userService.ForgotPasswordEmail(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Resets the user's password using the provided reset token and new password.
    /// </summary>
    /// <param name="request">The request containing the reset token, email, and new password.</param>
    /// <returns>
    /// Returns 200 OK if the password was reset successfully; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [HttpPost(nameof(ResetPassword))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordAsync(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Retrieves the profile information of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// Returns 200 OK with the user's profile data if successful; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [Authorize, HttpGet(nameof(GetProfile))]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(User.GetUserId());
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Updates the profile information of the currently authenticated user.
    /// </summary>
    /// <param name="request">The request containing updated profile information, including optional file uploads.</param>
    /// <returns>
    /// Returns 200 OK if the profile was updated successfully; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [Authorize, HttpPut(nameof(UpdateProfile))]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserRequest request)
    {
        var result = await _userService.UpdateProfileAsync(request, User.GetUserId());
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user to be deleted.</param>
    /// <returns>
    /// Returns 200 OK if the user was deleted successfully; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [HttpDelete(nameof(DeleteUser))]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Resends the email confirmation message to the user.
    /// </summary>
    /// <param name="request">The request containing the user's email address.</param>
    /// <returns>
    /// Returns 200 OK if the confirmation email was sent successfully; otherwise, returns 400 Bad Request with an error message.
    /// </returns>
    [HttpPost(nameof(ResendEmail))]
    public async Task<IActionResult> ResendEmail(ResendEmailConfirmation request)
    {
        var result = await _userService.ResendEmailAsync(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result.Message);
    }

    /// <summary>
    /// Get Support Request setting for the authenticated user's tenant.
    /// </summary>
    /// <returns>An action result indicating the toggle operation success status, or a bad request with error message.</returns>
    [Authorize, HttpGet(nameof(GetSupportRequest))]
    public async Task<IActionResult> GetSupportRequest()
    {
        var result = await _userService.GetSupportRequest(User.GetTenantId());
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Toggles the Support Request setting for the authenticated user's tenant.
    /// </summary>
    /// <returns>An action result indicating the toggle operation success status, or a bad request with error message.</returns>
    [Authorize, HttpPost(nameof(ToggleSupportRequest))]
    public async Task<IActionResult> ToggleSupportRequest()
    {
        var result = await _userService.ToggleSupportRequest(User.GetTenantId());
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result.Message);
    }

    /// <summary>
    /// Get service prices for the authenticated user's tenant.
    /// </summary>
    /// <returns>Returns service prices for the authenticated user's tenant.</returns>
    [Authorize, HttpGet(nameof(GetServicePrices))]
    public async Task<IActionResult> GetServicePrices()
    {
        var result = await _servicePriceService.GetServicePriceByTenantIdOrDafault(User.GetTenantId());
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result.Message);
    }
}
