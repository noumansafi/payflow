using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Transactions.Queries.GetTransactionByReference;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Transactions;

public sealed class GetTransactionByReferenceQueryHandlerTests
{
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetTransactionByReferenceQueryHandler CreateSut() =>
        new(_transactions, _wallets, _currentUser);

    [Fact]
    public async Task Handle_WhenOwnerAsReceiver_ReturnsReceivedDirection()
    {
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId);
        var senderWalletId = Guid.NewGuid();
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "PF-REF-99",
            SenderWalletId = senderWalletId,
            ReceiverWalletId = wallet.Id,
            Amount = 5m,
            Fee = 0m,
            Status = TransactionStatus.Completed,
            TransactionType = TransactionType.Transfer,
            CreatedAtUtc = DateTime.UtcNow,
            CompletedAtUtc = DateTime.UtcNow
        };

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _transactions.GetByReferenceNumberAsync("PF-REF-99", Arg.Any<CancellationToken>()).Returns(tx);

        var result = await CreateSut().Handle(
            new GetTransactionByReferenceQuery("PF-REF-99"),
            CancellationToken.None);

        result.Direction.Should().Be(nameof(TransactionDirection.Received));
        result.CounterpartyWalletId.Should().Be(senderWalletId);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(
            new GetTransactionByReferenceQuery("PF-REF-99"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
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
}
