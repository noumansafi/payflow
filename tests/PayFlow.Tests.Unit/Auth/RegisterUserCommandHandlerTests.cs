using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Auth.Commands.RegisterUser;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Auth;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RegisterUserCommandHandler CreateSut() =>
        new(_users, _passwordHasher, _tokenService, _emailSender, _auditLogger, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_WhenEmailExists_ThrowsConflict()
    {
        _users.EmailExistsAsync("jane@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var act = () => CreateSut().Handle(
            new RegisterUserCommand("jane@example.com", "Password1", "Jane", "Doe"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesUserWalletAndAudit()
    {
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        _clock.UtcNow.Returns(now);
        _users.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash("Password1").Returns("hashed");
        _tokenService.CreateRefreshToken().Returns("verify-token");
        _tokenService.HashToken("verify-token").Returns("verify-hash");

        User? captured = null;
        _users.When(x => x.Add(Arg.Any<User>())).Do(ci => captured = ci.Arg<User>());

        var result = await CreateSut().Handle(
            new RegisterUserCommand("Jane@Example.com", "Password1", "Jane", "Doe"),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Email.Should().Be("jane@example.com");
        captured.PasswordHash.Should().Be("hashed");
        captured.Wallet.Should().NotBeNull();
        captured.Wallet!.Balance.Should().Be(0m);
        captured.Wallet.Currency.Should().Be("USD");
        captured.Wallet.Status.Should().Be(WalletStatus.Active);
        captured.EmailVerificationTokenHash.Should().Be("verify-hash");

        result.EmailVerificationToken.Should().Be("verify-token");
        result.User.Email.Should().Be("jane@example.com");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _auditLogger.Received(1).WriteAsync(
            AuditAction.Register,
            nameof(User),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendEmailAsync(
            "jane@example.com",
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("verify-token")),
            Arg.Any<CancellationToken>());
    }
}
