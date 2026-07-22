using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Transfers.Commands.TransferMoney;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Transfers;

public sealed class TransferMoneyCommandHandlerTests
{
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly IReferenceNumberGenerator _referenceNumbers = Substitute.For<IReferenceNumberGenerator>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private TransferMoneyCommandHandler CreateSut() =>
        new(_wallets, _transactions, _notifications, _referenceNumbers,
            _currentUser, _auditLogger, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_WhenValid_TransfersAtomicallyAndSideEffects()
    {
        var now = new DateTime(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc);
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var sender = CreateWallet(senderId, 100m, WalletStatus.Active);
        var receiver = CreateWallet(receiverId, 10m, WalletStatus.Active);

        _clock.UtcNow.Returns(now);
        _currentUser.UserId.Returns(senderId);
        _referenceNumbers.Next().Returns("PF-TEST-001");
        _wallets.GetByUserIdAsync(senderId, Arg.Any<CancellationToken>()).Returns(sender);
        _wallets.GetByUserIdAsync(receiverId, Arg.Any<CancellationToken>()).Returns(receiver);

        Transaction? capturedTx = null;
        _transactions.When(x => x.Add(Arg.Any<Transaction>()))
            .Do(ci => capturedTx = ci.Arg<Transaction>());

        var notifications = new List<Notification>();
        _notifications.When(x => x.Add(Arg.Any<Notification>()))
            .Do(ci => notifications.Add(ci.Arg<Notification>()));

        var result = await CreateSut().Handle(
            new TransferMoneyCommand(receiverId, 25.50m, "lunch"),
            CancellationToken.None);

        sender.Balance.Should().Be(74.50m);
        receiver.Balance.Should().Be(35.50m);
        result.ReferenceNumber.Should().Be("PF-TEST-001");
        result.Amount.Should().Be(25.50m);
        result.Fee.Should().Be(0m);
        result.Status.Should().Be(nameof(TransactionStatus.Completed));

        capturedTx.Should().NotBeNull();
        capturedTx!.SenderWalletId.Should().Be(sender.Id);
        capturedTx.ReceiverWalletId.Should().Be(receiver.Id);
        capturedTx.Note.Should().Be("lunch");
        capturedTx.Status.Should().Be(TransactionStatus.Completed);

        notifications.Should().HaveCount(2);
        notifications.Should().Contain(n => n.Type == NotificationType.TransferSent && n.UserId == senderId);
        notifications.Should().Contain(n => n.Type == NotificationType.TransferReceived && n.UserId == receiverId);

        await _auditLogger.Received(1).WriteAsync(
            AuditAction.Transfer,
            "Transaction",
            capturedTx.Id,
            senderId,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSelfTransfer_ThrowsValidation()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);

        var act = () => CreateSut().Handle(
            new TransferMoneyCommand(userId, 10m, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReceiverMissing_ThrowsNotFound()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var sender = CreateWallet(senderId, 100m, WalletStatus.Active);

        _currentUser.UserId.Returns(senderId);
        _wallets.GetByUserIdAsync(senderId, Arg.Any<CancellationToken>()).Returns(sender);
        _wallets.GetByUserIdAsync(receiverId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var act = () => CreateSut().Handle(
            new TransferMoneyCommand(receiverId, 10m, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Receiver*");
    }

    [Fact]
    public async Task Handle_WhenSenderFrozen_ThrowsConflict()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var sender = CreateWallet(senderId, 100m, WalletStatus.Frozen);
        var receiver = CreateWallet(receiverId, 0m, WalletStatus.Active);

        _currentUser.UserId.Returns(senderId);
        _wallets.GetByUserIdAsync(senderId, Arg.Any<CancellationToken>()).Returns(sender);
        _wallets.GetByUserIdAsync(receiverId, Arg.Any<CancellationToken>()).Returns(receiver);

        var act = () => CreateSut().Handle(
            new TransferMoneyCommand(receiverId, 10m, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*active*");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReceiverFrozen_ThrowsConflict()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var sender = CreateWallet(senderId, 100m, WalletStatus.Active);
        var receiver = CreateWallet(receiverId, 0m, WalletStatus.Frozen);

        _currentUser.UserId.Returns(senderId);
        _wallets.GetByUserIdAsync(senderId, Arg.Any<CancellationToken>()).Returns(sender);
        _wallets.GetByUserIdAsync(receiverId, Arg.Any<CancellationToken>()).Returns(receiver);

        var act = () => CreateSut().Handle(
            new TransferMoneyCommand(receiverId, 10m, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*active*");
    }

    [Fact]
    public async Task Handle_WhenInsufficientBalance_ThrowsConflict()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var sender = CreateWallet(senderId, 5m, WalletStatus.Active);
        var receiver = CreateWallet(receiverId, 0m, WalletStatus.Active);

        _currentUser.UserId.Returns(senderId);
        _wallets.GetByUserIdAsync(senderId, Arg.Any<CancellationToken>()).Returns(sender);
        _wallets.GetByUserIdAsync(receiverId, Arg.Any<CancellationToken>()).Returns(receiver);

        var act = () => CreateSut().Handle(
            new TransferMoneyCommand(receiverId, 10m, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Insufficient*");
        sender.Balance.Should().Be(5m);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(
            new TransferMoneyCommand(Guid.NewGuid(), 10m, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    private static Wallet CreateWallet(Guid userId, decimal balance, WalletStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Balance = balance,
        Currency = "USD",
        Status = status,
        CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
    };
}
