using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(PayFlowDbContext dbContext) : ITransactionRepository
{
    public void Add(Transaction transaction) => dbContext.Transactions.Add(transaction);

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Transaction?> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default) =>
        dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task<TransactionListResult> ListForWalletAsync(
        TransactionListQuery query,
        CancellationToken cancellationToken = default)
    {
        var source = dbContext.Transactions.AsNoTracking().Where(x =>
            x.SenderWalletId == query.WalletId || x.ReceiverWalletId == query.WalletId);

        if (query.Status is { } status)
        {
            source = source.Where(x => x.Status == status);
        }

        if (query.Direction == TransactionDirection.Sent)
        {
            source = source.Where(x => x.SenderWalletId == query.WalletId);
        }
        else if (query.Direction == TransactionDirection.Received)
        {
            source = source.Where(x => x.ReceiverWalletId == query.WalletId);
        }

        if (!string.IsNullOrWhiteSpace(query.ReferenceNumber))
        {
            source = source.Where(x => x.ReferenceNumber == query.ReferenceNumber);
        }

        if (query.FromUtc is { } fromUtc)
        {
            source = source.Where(x => x.CreatedAtUtc >= fromUtc);
        }

        if (query.ToUtc is { } toUtc)
        {
            source = source.Where(x => x.CreatedAtUtc <= toUtc);
        }

        var totalCount = await source.CountAsync(cancellationToken);

        // Stable sort: CreatedAt + Id tie-breaker for deterministic pagination.
        source = query.SortDescending
            ? source.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            : source.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id);

        var items = await source
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync(cancellationToken);

        return new TransactionListResult(items, totalCount);
    }
}
