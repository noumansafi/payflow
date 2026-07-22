using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Transfers;

/// <summary>
/// Core P2P transfer invariants evaluated before balances move.
/// </summary>
public static class TransferRules
{
    public static bool IsPositiveAmount(decimal amount) => amount > 0m;

    public static bool IsSelfTransfer(Guid senderUserId, Guid receiverUserId) =>
        senderUserId == receiverUserId;

    public static bool AreWalletsTransferable(Wallet sender, Wallet receiver) =>
        sender.Status == WalletStatus.Active && receiver.Status == WalletStatus.Active;

    public static bool HasSufficientBalance(Wallet sender, decimal amount) =>
        sender.Balance >= amount;
}
