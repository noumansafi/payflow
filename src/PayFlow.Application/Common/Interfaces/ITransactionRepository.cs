using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface ITransactionRepository
{
    void Add(Transaction transaction);

    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);
}
