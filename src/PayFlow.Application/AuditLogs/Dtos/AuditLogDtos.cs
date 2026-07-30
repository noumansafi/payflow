namespace PayFlow.Application.AuditLogs.Dtos;

public sealed record AuditLogDto(
    Guid Id,
    Guid? ActorUserId,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? Metadata,
    string? IpAddress,
    DateTime CreatedAtUtc);
