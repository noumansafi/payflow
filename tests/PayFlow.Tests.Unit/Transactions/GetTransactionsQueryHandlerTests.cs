using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Transactions.Queries.GetTransactions;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Transactions;

public sealed class GetTransactionsQueryHandlerTests
{
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetTransactionsQueryHandler CreateSut() =>
        new(_transactions, _wallets, _currentUser);

    [Fact]
    public async Task Handle_WhenOwner_ReturnsPagedMappedItems()
    {
        var userId = Guid.NewGuid();
        var wallet = CreateWallet(userId);
        var otherWalletId = Guid.NewGuid();
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "PF-LIST-1",
            SenderWalletId = wallet.Id,
            ReceiverWalletId = otherWalletId,
            Amount = 12m,
            Fee = 0m,
            Status = TransactionStatus.Completed,
            TransactionType = TransactionType.Transfer,
            CreatedAtUtc = DateTime.UtcNow,
            CompletedAtUtc = DateTime.UtcNow
        };

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _transactions.ListForWalletAsync(Arg.Any<TransactionListQuery>(), Arg.Any<CancellationToken>())
            .Returns(new TransactionListResult([tx], 1));

        var result = await CreateSut().Handle(new GetTransactionsQuery(Page: 1, PageSize: 20), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Direction.Should().Be(nameof(TransactionDirection.Sent));
        result.Items[0].CounterpartyWalletId.Should().Be(otherWalletId);

        await _transactions.Received(1).ListForWalletAsync(
            Arg.Is<TransactionListQuery>(q =>
                q.WalletId == wallet.Id &&
                q.Skip == 0 &&
                q.Take == 20 &&
                q.SortDescending),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new GetTransactionsQuery(), CancellationToken.None);

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
