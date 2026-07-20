using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Infrastructure.Persistence;

public sealed class UnitOfWork(PayFlowDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
