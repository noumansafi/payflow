using FluentAssertions;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Transfers;

namespace PayFlow.Tests.Unit.Transfers;

public sealed class TransferRulesTests
{
    [Theory]
    [InlineData(0.01, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsPositiveAmount_EvaluatesCorrectly(decimal amount, bool expected) =>
        TransferRules.IsPositiveAmount(amount).Should().Be(expected);

    [Fact]
    public void IsSelfTransfer_WhenSameUser_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        TransferRules.IsSelfTransfer(id, id).Should().BeTrue();
    }

    [Fact]
    public void AreWalletsTransferable_WhenEitherFrozen_ReturnsFalse()
    {
        var sender = Wallet(WalletStatus.Frozen, 50m);
        var receiver = Wallet(WalletStatus.Active, 0m);

        TransferRules.AreWalletsTransferable(sender, receiver).Should().BeFalse();
    }

    [Fact]
    public void HasSufficientBalance_WhenExact_ReturnsTrue()
    {
        var sender = Wallet(WalletStatus.Active, 25.50m);
        TransferRules.HasSufficientBalance(sender, 25.50m).Should().BeTrue();
    }

    [Fact]
    public void HasSufficientBalance_WhenShort_ReturnsFalse()
    {
        var sender = Wallet(WalletStatus.Active, 10m);
        TransferRules.HasSufficientBalance(sender, 10.01m).Should().BeFalse();
    }

    private static Wallet Wallet(WalletStatus status, decimal balance) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Balance = balance,
            Currency = "USD",
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };
}
