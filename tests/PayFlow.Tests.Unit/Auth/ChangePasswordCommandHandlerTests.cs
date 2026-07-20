using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Auth.Commands.ChangePassword;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Auth;

public sealed class ChangePasswordCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ChangePasswordCommandHandler CreateSut() =>
        new(_users, _refreshTokens, _passwordHasher, _currentUser, _auditLogger, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_WhenValid_UpdatesPasswordAndRevokesSessions()
    {
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "jane@example.com",
            PasswordHash = "old-hash",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = now
        };

        _clock.UtcNow.Returns(now);
        _currentUser.UserId.Returns(user.Id);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("old-hash", "Password1").Returns(true);
        _passwordHasher.Hash("Password2").Returns("new-hash");

        await CreateSut().Handle(
            new ChangePasswordCommand("Password1", "Password2"),
            CancellationToken.None);

        user.PasswordHash.Should().Be("new-hash");
        await _refreshTokens.Received(1).RevokeAllActiveForUserAsync(
            user.Id,
            now,
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInactive_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "jane@example.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User,
            IsActive = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _currentUser.UserId.Returns(user.Id);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = () => CreateSut().Handle(
            new ChangePasswordCommand("Password1", "Password2"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }
}
