using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

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
}
