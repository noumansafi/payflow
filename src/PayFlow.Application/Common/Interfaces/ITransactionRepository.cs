using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface ITransactionRepository
{
    void Add(Transaction transaction);
}
