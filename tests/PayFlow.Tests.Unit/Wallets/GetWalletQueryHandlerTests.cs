using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Wallets.Queries.GetWallet;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Wallets;

public sealed class GetWalletQueryHandlerTests
{
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetWalletQueryHandler CreateSut() => new(_wallets, _currentUser);

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new GetWalletQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
        await _wallets.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletMissing_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var act = () => CreateSut().Handle(new GetWalletQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Wallet*");
    }

    [Fact]
    public async Task Handle_WhenWalletExists_ReturnsDto_WithoutMutating()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 125.50m,
            Currency = "USD",
            Status = WalletStatus.Frozen,
            CreatedAtUtc = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc)
        };

        _currentUser.UserId.Returns(userId);
        _wallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await CreateSut().Handle(new GetWalletQuery(), CancellationToken.None);

        result.Id.Should().Be(wallet.Id);
        result.UserId.Should().Be(userId);
        result.Balance.Should().Be(125.50m);
        result.Currency.Should().Be("USD");
        result.Status.Should().Be(nameof(WalletStatus.Frozen));
        result.CreatedAtUtc.Should().Be(wallet.CreatedAtUtc);

        // Query must only read for the authenticated owner.
        await _wallets.Received(1).GetByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }
}
