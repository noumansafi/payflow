using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using PayFlow.Application.Auth.Commands.RefreshToken;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Options;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Auth;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOptions<JwtOptions> _jwtOptions = Options.Create(new JwtOptions
    {
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7
    });

    private RefreshTokenCommandHandler CreateSut() =>
        new(_refreshTokens, _users, _tokenService, _clock, _unitOfWork, _jwtOptions);

    [Fact]
    public async Task Handle_WhenTokenRevoked_ThrowsUnauthorized()
    {
        var now = DateTime.UtcNow;
        _clock.UtcNow.Returns(now);
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.GetByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "hash",
            CreatedAtUtc = now.AddDays(-1),
            ExpiresAtUtc = now.AddDays(1),
            RevokedAtUtc = now.AddMinutes(-1)
        });

        var act = () => CreateSut().Handle(new RefreshTokenCommand("raw"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task Handle_WhenValid_RotatesRefreshToken()
    {
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();
        var existing = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "old-hash",
            CreatedAtUtc = now.AddDays(-1),
            ExpiresAtUtc = now.AddDays(6)
        };

        var user = new User
        {
            Id = userId,
            Email = "jane@example.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = now
        };

        _clock.UtcNow.Returns(now);
        _tokenService.HashToken("old-raw").Returns("old-hash");
        _tokenService.CreateRefreshToken().Returns("new-raw");
        _tokenService.HashToken("new-raw").Returns("new-hash");
        _tokenService.CreateAccessToken(user).Returns("access");
        _refreshTokens.GetByHashAsync("old-hash", Arg.Any<CancellationToken>()).Returns(existing);
        _users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await CreateSut().Handle(new RefreshTokenCommand("old-raw"), CancellationToken.None);

        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("new-raw");
        existing.RevokedAtUtc.Should().Be(now);
        existing.ReplacedByTokenHash.Should().Be("new-hash");
        _refreshTokens.Received(1).Add(Arg.Is<RefreshToken>(t => t.TokenHash == "new-hash"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
