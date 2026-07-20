using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using PayFlow.Application.Auth.Commands.Login;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Options;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Auth;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOptions<JwtOptions> _jwtOptions = Options.Create(new JwtOptions
    {
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7
    });

    private LoginCommandHandler CreateSut() =>
        new(_users, _refreshTokens, _passwordHasher, _tokenService, _auditLogger, _clock, _unitOfWork, _jwtOptions);

    [Fact]
    public async Task Handle_WhenUserMissing_ThrowsUnauthorized()
    {
        _users.GetByEmailAsync("missing@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => CreateSut().Handle(
            new LoginCommand("missing@example.com", "Password1"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ThrowsUnauthorized()
    {
        var user = CreateUser();
        _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, "WrongPass1").Returns(false);

        var act = () => CreateSut().Handle(
            new LoginCommand(user.Email, "WrongPass1"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ReturnsTokensAndStoresRefreshHash()
    {
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var user = CreateUser();
        _clock.UtcNow.Returns(now);
        _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, "Password1").Returns(true);
        _tokenService.CreateAccessToken(user).Returns("access-token");
        _tokenService.CreateRefreshToken().Returns("refresh-token");
        _tokenService.HashToken("refresh-token").Returns("refresh-hash");

        RefreshToken? captured = null;
        _refreshTokens.When(x => x.Add(Arg.Any<RefreshToken>())).Do(ci => captured = ci.Arg<RefreshToken>());

        var result = await CreateSut().Handle(
            new LoginCommand(user.Email, "Password1", "127.0.0.1"),
            CancellationToken.None);

        result.Tokens.AccessToken.Should().Be("access-token");
        result.Tokens.RefreshToken.Should().Be("refresh-token");
        captured.Should().NotBeNull();
        captured!.TokenHash.Should().Be("refresh-hash");
        captured.UserId.Should().Be(user.Id);
        captured.CreatedByIp.Should().Be("127.0.0.1");

        await _auditLogger.Received(1).WriteAsync(
            AuditAction.Login,
            nameof(User),
            user.Id,
            user.Id,
            Arg.Any<string?>(),
            "127.0.0.1",
            Arg.Any<CancellationToken>());
    }

    private static User CreateUser() =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = "jane@example.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        };
}
