using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Repositories;

public sealed class WalletRepository(PayFlowDbContext dbContext) : IWalletRepository
{
    public Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Wallets.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default) =>
        dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == walletId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, string>> GetOwnerDisplayNamesByWalletIdsAsync(
        IReadOnlyCollection<Guid> walletIds,
        CancellationToken cancellationToken = default)
    {
        if (walletIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var ids = walletIds.Distinct().ToArray();

        var rows = await dbContext.Wallets
            .AsNoTracking()
            .Where(w => ids.Contains(w.Id))
            .Select(w => new
            {
                w.Id,
                w.User!.FirstName,
                w.User.LastName
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.Id,
            x => $"{x.FirstName} {x.LastName}".Trim());
    }
}
