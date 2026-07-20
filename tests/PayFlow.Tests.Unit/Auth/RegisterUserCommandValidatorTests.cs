using FluentAssertions;
using PayFlow.Application.Auth.Commands.RegisterUser;

namespace PayFlow.Tests.Unit.Auth;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenPasswordTooWeak_Fails()
    {
        var result = await _validator.ValidateAsync(
            new RegisterUserCommand("jane@example.com", "password", "Jane", "Doe"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public async Task Validate_WhenRequestValid_Passes()
    {
        var result = await _validator.ValidateAsync(
            new RegisterUserCommand("jane@example.com", "Password1", "Jane", "Doe"));

        result.IsValid.Should().BeTrue();
    }
}
