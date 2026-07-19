using PayFlow.Domain.Common;
using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Entities;

public sealed class AuditLog : Entity
{
    public Guid? ActorUserId { get; set; }
    public AuditAction Action { get; set; }
    public required string EntityType { get; set; }
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Structured JSON metadata. Must never contain passwords or raw tokens.
    /// </summary>
    public string? Metadata { get; set; }

    public string? IpAddress { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
