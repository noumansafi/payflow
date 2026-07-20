using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Auth.Commands.ForgotPassword;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Auth;

public sealed class ForgotPasswordCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ForgotPasswordCommandHandler CreateSut() =>
        new(_users, _tokenService, _emailSender, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_WhenUserMissing_DoesNotRevealAccountExistence()
    {
        _users.GetByEmailAsync("missing@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await CreateSut().Handle(
            new ForgotPasswordCommand("missing@example.com"),
            CancellationToken.None);

        result.Message.Should().Contain("If an account exists");
        result.ResetToken.Should().BeNull();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserExists_IssuesResetToken()
    {
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "jane@example.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = now
        };

        _clock.UtcNow.Returns(now);
        _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _tokenService.CreateRefreshToken().Returns("reset-token");
        _tokenService.HashToken("reset-token").Returns("reset-hash");

        var result = await CreateSut().Handle(
            new ForgotPasswordCommand(user.Email),
            CancellationToken.None);

        result.ResetToken.Should().Be("reset-token");
        user.PasswordResetTokenHash.Should().Be("reset-hash");
        user.PasswordResetTokenExpiresAtUtc.Should().Be(now.AddHours(1));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
