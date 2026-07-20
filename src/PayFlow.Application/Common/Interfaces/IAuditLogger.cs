using PayFlow.Domain.Enums;

namespace PayFlow.Application.Common.Interfaces;

public interface IAuditLogger
{
    Task WriteAsync(
        AuditAction action,
        string entityType,
        Guid? entityId,
        Guid? actorUserId,
        string? metadata = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}
