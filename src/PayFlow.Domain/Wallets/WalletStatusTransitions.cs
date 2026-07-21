using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Wallets;

/// <summary>
/// Self-service status transitions allowed for the wallet owner.
/// System-driven statuses (e.g. Suspended, Closed) stay out of this set
/// and will use admin/internal commands when introduced.
/// </summary>
public static class WalletStatusTransitions
{
    private static readonly HashSet<(WalletStatus From, WalletStatus To)> UserAllowed =
    [
        (WalletStatus.Active, WalletStatus.Frozen),
        (WalletStatus.Frozen, WalletStatus.Active)
    ];

    public static bool IsUserAllowed(WalletStatus from, WalletStatus to) =>
        UserAllowed.Contains((from, to));
}
