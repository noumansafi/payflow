using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Common.Interfaces;

public interface ITransactionRepository
{
    void Add(Transaction transaction);

    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);

    Task<TransactionListResult> ListForWalletAsync(
        TransactionListQuery query,
        CancellationToken cancellationToken = default);
}

public sealed record TransactionListQuery(
    Guid WalletId,
    TransactionStatus? Status,
    TransactionDirection? Direction,
    string? ReferenceNumber,
    DateTime? FromUtc,
    DateTime? ToUtc,
    bool SortDescending,
    int Skip,
    int Take);

public sealed record TransactionListResult(
    IReadOnlyList<Transaction> Items,
    int TotalCount);
