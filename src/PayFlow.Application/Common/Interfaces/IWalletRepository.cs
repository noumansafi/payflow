using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves wallet owner display names (FirstName + LastName) keyed by wallet id.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetOwnerDisplayNamesByWalletIdsAsync(
        IReadOnlyCollection<Guid> walletIds,
        CancellationToken cancellationToken = default);
}
