using FluentAssertions;
using PayFlow.Infrastructure.Auth;

namespace PayFlow.Tests.Unit.Auth;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Verify_WhenPasswordMatches_ReturnsTrue()
    {
        var hash = _hasher.Hash("Password1");

        _hasher.Verify(hash, "Password1").Should().BeTrue();
    }

    [Fact]
    public void Verify_WhenPasswordDoesNotMatch_ReturnsFalse()
    {
        var hash = _hasher.Hash("Password1");

        _hasher.Verify(hash, "Password2").Should().BeFalse();
    }
}
