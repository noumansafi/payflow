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
}
