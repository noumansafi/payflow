using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface IBeneficiaryRepository
{
    Task<bool> ExistsAsync(
        Guid ownerUserId,
        Guid beneficiaryUserId,
        CancellationToken cancellationToken = default);

    Task<Beneficiary?> GetByIdForOwnerAsync(
        Guid id,
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    Task<BeneficiaryListResult> ListForOwnerAsync(
        Guid ownerUserId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    void Add(Beneficiary beneficiary);

    void Remove(Beneficiary beneficiary);
}

public sealed record BeneficiaryListResult(
    IReadOnlyList<Beneficiary> Items,
    int TotalCount);
