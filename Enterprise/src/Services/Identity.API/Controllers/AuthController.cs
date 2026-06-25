using System.Security.Claims;
using Identity.Application.Auth.Commands;
using Identity.Application.Auth.Queries;
using Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokenDto>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken));

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new RegisterCommand(request.TenantId, request.Email, request.Password, request.FirstName, request.LastName), cancellationToken));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokenDto>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken));

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var user = await mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
