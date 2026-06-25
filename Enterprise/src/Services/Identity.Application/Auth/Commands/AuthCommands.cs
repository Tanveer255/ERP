using BuildingBlocks.Application;
using FluentValidation;
using Identity.Application.DTOs;

namespace Identity.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : ICommand<AuthTokenDto>;
public record RegisterCommand(Guid TenantId, string Email, string Password, string FirstName, string LastName) : ICommand<UserDto>;
public record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokenDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
