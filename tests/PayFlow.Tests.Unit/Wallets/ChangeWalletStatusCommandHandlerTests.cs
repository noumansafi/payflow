using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Wallets.Commands.ChangeWalletStatus;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Wallets;

public sealed class ChangeWalletStatusCommandHandlerTests
{
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ChangeWalletStatusCommandHandler CreateSut() =>
        new(_wallets, _currentUser, _auditLogger, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_WhenActiveToFrozen_UpdatesAndAudits()
    {
        var now = new DateTime(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId, WalletStatus.Active);

        _clock.UtcNow.Returns(now);
        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await CreateSut().Handle(
            new ChangeWalletStatusCommand(WalletStatus.Frozen),
            CancellationToken.None);

        result.Status.Should().Be(nameof(WalletStatus.Frozen));
        wallet.Status.Should().Be(WalletStatus.Frozen);
        wallet.UpdatedAtUtc.Should().Be(now);

        await _auditLogger.Received(1).WriteAsync(
            AuditAction.WalletFreeze,
            "Wallet",
            wallet.Id,
            userId,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFrozenToActive_UpdatesAndAudits()
    {
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId, WalletStatus.Frozen);

        _clock.UtcNow.Returns(DateTime.UtcNow);
        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await CreateSut().Handle(
            new ChangeWalletStatusCommand(WalletStatus.Active),
            CancellationToken.None);

        result.Status.Should().Be(nameof(WalletStatus.Active));
        wallet.Status.Should().Be(WalletStatus.Active);

        await _auditLogger.Received(1).WriteAsync(
            AuditAction.WalletActivation,
            "Wallet",
            wallet.Id,
            userId,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyTargetStatus_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId, WalletStatus.Frozen);

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);

        var act = () => CreateSut().Handle(
            new ChangeWalletStatusCommand(WalletStatus.Frozen),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already frozen*");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(
            new ChangeWalletStatusCommand(WalletStatus.Frozen),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task Handle_WhenWalletMissing_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var act = () => CreateSut().Handle(
            new ChangeWalletStatusCommand(WalletStatus.Frozen),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Wallet CreateWallet(Guid userId, WalletStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Balance = 50m,
        Currency = "USD",
        Status = status,
        CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
    };
}
