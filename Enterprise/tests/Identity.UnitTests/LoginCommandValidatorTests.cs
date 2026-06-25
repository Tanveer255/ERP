using FluentValidation.TestHelper;
using Identity.Application.Auth.Commands;

namespace Identity.UnitTests;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_email_empty()
    {
        var result = _validator.TestValidate(new LoginCommand("", "Password123!"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_pass_with_valid_input()
    {
        var result = _validator.TestValidate(new LoginCommand("user@example.com", "Password123!"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
