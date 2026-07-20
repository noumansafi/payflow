using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Auth.Commands.Logout;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Tests.Unit.Auth;

public sealed class LogoutCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private LogoutCommandHandler CreateSut() =>
        new(_refreshTokens, _tokenService, _auditLogger, _currentUser, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_WhenTokenBelongsToAnotherUser_DoesNotRevokeIt()
    {
        var now = DateTime.UtcNow;
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            TokenHash = "hash",
            CreatedAtUtc = now.AddDays(-1),
            ExpiresAtUtc = now.AddDays(1)
        };

        _clock.UtcNow.Returns(now);
        _currentUser.UserId.Returns(currentUserId);
        _tokenService.HashToken("raw").Returns("hash");
        _refreshTokens.GetByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        await CreateSut().Handle(new LogoutCommand("raw"), CancellationToken.None);

        token.RevokedAtUtc.Should().BeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoRefreshToken_RevokesAllSessions()
    {
        var now = DateTime.UtcNow;
        var userId = Guid.NewGuid();
        _clock.UtcNow.Returns(now);
        _currentUser.UserId.Returns(userId);

        await CreateSut().Handle(new LogoutCommand(null), CancellationToken.None);

        await _refreshTokens.Received(1).RevokeAllActiveForUserAsync(
            userId,
            now,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new LogoutCommand(null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }
}
