using PayFlow.Domain.Common;
using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Entities;

public sealed class Wallet : AuditableEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public required string Currency { get; set; }
    public WalletStatus Status { get; set; } = WalletStatus.Active;

    /// <summary>
    /// Optimistic concurrency token to protect balance updates under concurrent transfers.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    public User? User { get; set; }
    public ICollection<Transaction> SentTransactions { get; set; } = [];
    public ICollection<Transaction> ReceivedTransactions { get; set; } = [];
}
