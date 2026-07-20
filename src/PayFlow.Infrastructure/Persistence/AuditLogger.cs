using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Infrastructure.Persistence;

public sealed class AuditLogger(
    PayFlowDbContext dbContext,
    IDateTimeProvider clock) : IAuditLogger
{
    public Task WriteAsync(
        AuditAction action,
        string entityType,
        Guid? entityId,
        Guid? actorUserId,
        string? metadata = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Metadata = metadata,
            IpAddress = ipAddress,
            CreatedAtUtc = clock.UtcNow
        });

        return Task.CompletedTask;
    }
}
