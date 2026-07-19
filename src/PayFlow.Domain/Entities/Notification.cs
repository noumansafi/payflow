using PayFlow.Domain.Common;
using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Entities;

public sealed class Notification : Entity
{
    public Guid UserId { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public User? User { get; set; }
}
