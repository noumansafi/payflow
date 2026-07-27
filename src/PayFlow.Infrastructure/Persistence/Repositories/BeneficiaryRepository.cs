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

    public Task<Beneficiary?> GetByIdForOwnerAsync(
        Guid id,
        Guid ownerUserId,
        CancellationToken cancellationToken = default) =>
        dbContext.Beneficiaries.FirstOrDefaultAsync(
            x => x.Id == id && x.OwnerUserId == ownerUserId,
            cancellationToken);

    public async Task<BeneficiaryListResult> ListForOwnerAsync(
        Guid ownerUserId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Beneficiaries
            .AsNoTracking()
            .Include(x => x.BeneficiaryUser)
            .Where(x => x.OwnerUserId == ownerUserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new BeneficiaryListResult(items, totalCount);
    }

    public void Add(Beneficiary beneficiary) => dbContext.Beneficiaries.Add(beneficiary);

    public void Remove(Beneficiary beneficiary) => dbContext.Beneficiaries.Remove(beneficiary);
}
