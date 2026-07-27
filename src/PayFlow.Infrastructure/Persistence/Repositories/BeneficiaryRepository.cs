using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Repositories;

public sealed class BeneficiaryRepository(PayFlowDbContext dbContext) : IBeneficiaryRepository
{
    public Task<bool> ExistsAsync(
        Guid ownerUserId,
        Guid beneficiaryUserId,
        CancellationToken cancellationToken = default) =>
        dbContext.Beneficiaries.AnyAsync(
            x => x.OwnerUserId == ownerUserId && x.BeneficiaryUserId == beneficiaryUserId,
            cancellationToken);

    public void Add(Beneficiary beneficiary) => dbContext.Beneficiaries.Add(beneficiary);
}
