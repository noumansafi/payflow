using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLogListResult> ListAsync(
        AuditAction? action,
        Guid? actorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}

public sealed record AuditLogListResult(
    IReadOnlyList<AuditLog> Items,
    int TotalCount);
