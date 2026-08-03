using FluentAssertions;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Wallets;

namespace PayFlow.Tests.Unit.Wallets;

public sealed class WalletStatusTransitionsTests
{
    [Theory]
    [InlineData(WalletStatus.Active, WalletStatus.Frozen, true)]
    [InlineData(WalletStatus.Frozen, WalletStatus.Active, true)]
    [InlineData(WalletStatus.Active, WalletStatus.Active, false)]
    [InlineData(WalletStatus.Frozen, WalletStatus.Frozen, false)]
    public void IsUserAllowed_OnlySelfServiceTransitions(
        WalletStatus from,
        WalletStatus to,
        bool expected) =>
        WalletStatusTransitions.IsUserAllowed(from, to).Should().Be(expected);
}
