using PayFlow.Domain.Common;
using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Entities;

public sealed class Transaction : Entity
{
    public required string ReferenceNumber { get; set; }
    public Guid SenderWalletId { get; set; }
    public Guid ReceiverWalletId { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public TransactionType TransactionType { get; set; } = TransactionType.Transfer;
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public Wallet? SenderWallet { get; set; }
    public Wallet? ReceiverWallet { get; set; }
}
