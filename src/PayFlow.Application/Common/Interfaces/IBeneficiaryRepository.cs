using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface IBeneficiaryRepository
{
    Task<bool> ExistsAsync(
        Guid ownerUserId,
        Guid beneficiaryUserId,
        CancellationToken cancellationToken = default);

    void Add(Beneficiary beneficiary);
}
