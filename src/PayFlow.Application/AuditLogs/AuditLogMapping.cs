using PayFlow.Application.AuditLogs.Dtos;
using PayFlow.Domain.Entities;

namespace PayFlow.Application.AuditLogs;

internal static class AuditLogMapping
{
    public static AuditLogDto ToDto(AuditLog log) =>
        new(
            log.Id,
            log.ActorUserId,
            log.Action.ToString(),
            log.EntityType,
            log.EntityId,
            log.Metadata,
            log.IpAddress,
            log.CreatedAtUtc);
}
