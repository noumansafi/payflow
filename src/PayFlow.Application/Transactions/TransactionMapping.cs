using PayFlow.Application.Transactions.Dtos;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Transactions;

internal static class TransactionMapping
{
    public static TransactionDto ToDto(Transaction transaction, Guid currentWalletId)
    {
        var isSent = transaction.SenderWalletId == currentWalletId;
        return new TransactionDto(
            transaction.Id,
            transaction.ReferenceNumber,
            isSent ? nameof(TransactionDirection.Sent) : nameof(TransactionDirection.Received),
            isSent ? transaction.ReceiverWalletId : transaction.SenderWalletId,
            transaction.Amount,
            transaction.Fee,
            transaction.Status.ToString(),
            transaction.TransactionType.ToString(),
            transaction.Note,
            transaction.CreatedAtUtc,
            transaction.CompletedAtUtc);
    }

    public static bool InvolvesWallet(Transaction transaction, Guid walletId) =>
        transaction.SenderWalletId == walletId || transaction.ReceiverWalletId == walletId;
}
