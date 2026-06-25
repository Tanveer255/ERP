using Identity.Application.Auth.Commands;
using Xunit;

namespace Identity.UnitTests;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_email_empty()
    {
        var result = _validator.Validate(new LoginCommand("", "Password123!"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Should_pass_with_valid_input()
    {
        var result = _validator.Validate(new LoginCommand("user@example.com", "Password123!"));
        Assert.True(result.IsValid);
    }
}
