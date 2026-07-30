using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Enums;

namespace PayFlow.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(PayFlowDbContext dbContext) : IAuditLogRepository
{
    public async Task<AuditLogListResult> ListAsync(
        AuditAction? action,
        Guid? actorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditLogs.AsNoTracking();

        if (action is { } actionFilter)
        {
            query = query.Where(x => x.Action == actionFilter);
        }

        if (actorUserId is { } actorFilter)
        {
            query = query.Where(x => x.ActorUserId == actorFilter);
        }

        if (fromUtc is { } from)
        {
            query = query.Where(x => x.CreatedAtUtc >= from);
        }

        if (toUtc is { } to)
        {
            query = query.Where(x => x.CreatedAtUtc <= to);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new AuditLogListResult(items, totalCount);
    }
}
