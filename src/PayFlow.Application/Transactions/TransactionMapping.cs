using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Transactions.Dtos;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Transactions;

internal static class TransactionMapping
{
    public static Guid CounterpartyWalletId(Transaction transaction, Guid currentWalletId) =>
        transaction.SenderWalletId == currentWalletId
            ? transaction.ReceiverWalletId
            : transaction.SenderWalletId;

    public static TransactionDto ToDto(
        Transaction transaction,
        Guid currentWalletId,
        string? counterpartyName = null)
    {
        var isSent = transaction.SenderWalletId == currentWalletId;
        return new TransactionDto(
            transaction.Id,
            transaction.ReferenceNumber,
            isSent ? nameof(TransactionDirection.Sent) : nameof(TransactionDirection.Received),
            isSent ? transaction.ReceiverWalletId : transaction.SenderWalletId,
            counterpartyName,
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

    public static async Task<IReadOnlyDictionary<Guid, string>> ResolveCounterpartyNamesAsync(
        IWalletRepository wallets,
        IEnumerable<Transaction> transactions,
        Guid currentWalletId,
        CancellationToken cancellationToken)
    {
        var walletIds = transactions
            .Select(tx => CounterpartyWalletId(tx, currentWalletId))
            .Distinct()
            .ToArray();

        return await wallets.GetOwnerDisplayNamesByWalletIdsAsync(walletIds, cancellationToken);
    }
}
