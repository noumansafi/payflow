using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Wallets.Queries.GetBalance;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Wallets;

public sealed class GetBalanceQueryHandlerTests
{
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetBalanceQueryHandler CreateSut() => new(_wallets, _currentUser);

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new GetBalanceQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task Handle_WhenWalletExists_ReturnsBalanceProjection()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 10m,
            Currency = "USD",
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await CreateSut().Handle(new GetBalanceQuery(), CancellationToken.None);

        result.WalletId.Should().Be(wallet.Id);
        result.Balance.Should().Be(10m);
        result.Currency.Should().Be("USD");
        result.Status.Should().Be(nameof(WalletStatus.Active));
    }
}
