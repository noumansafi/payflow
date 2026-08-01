using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Transactions.Queries.GetTransactionById;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Transactions;

public sealed class GetTransactionByIdQueryHandlerTests
{
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetTransactionByIdQueryHandler CreateSut() =>
        new(_transactions, _wallets, _currentUser);

    [Fact]
    public async Task Handle_WhenOwner_ReturnsDtoWithDirection()
    {
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId);
        var otherWalletId = Guid.NewGuid();
        var tx = CreateTransaction(wallet.Id, otherWalletId);

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _transactions.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>()).Returns(tx);
        _wallets.GetOwnerDisplayNamesByWalletIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(otherWalletId)),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [otherWalletId] = "Sara Khan" });

        var result = await CreateSut().Handle(new GetTransactionByIdQuery(tx.Id), CancellationToken.None);

        result.Id.Should().Be(tx.Id);
        result.Direction.Should().Be(nameof(TransactionDirection.Sent));
        result.CounterpartyWalletId.Should().Be(otherWalletId);
        result.CounterpartyName.Should().Be("Sara Khan");
        result.ReferenceNumber.Should().Be(tx.ReferenceNumber);
    }

    [Fact]
    public async Task Handle_WhenNotInvolved_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId);
        var tx = CreateTransaction(Guid.NewGuid(), Guid.NewGuid());

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _transactions.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>()).Returns(tx);

        var act = () => CreateSut().Handle(new GetTransactionByIdQuery(tx.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenMissing_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId);
        var txId = Guid.NewGuid();

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _transactions.GetByIdAsync(txId, Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var act = () => CreateSut().Handle(new GetTransactionByIdQuery(txId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Wallet CreateWallet(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Balance = 100m,
        Currency = "USD",
        Status = WalletStatus.Active,
        CreatedAtUtc = DateTime.UtcNow
    };

    private static Transaction CreateTransaction(Guid senderWalletId, Guid receiverWalletId) => new()
    {
        Id = Guid.NewGuid(),
        ReferenceNumber = "PF-TEST-001",
        SenderWalletId = senderWalletId,
        ReceiverWalletId = receiverWalletId,
        Amount = 10m,
        Fee = 0m,
        Status = TransactionStatus.Completed,
        TransactionType = TransactionType.Transfer,
        CreatedAtUtc = DateTime.UtcNow,
        CompletedAtUtc = DateTime.UtcNow
    };
}
