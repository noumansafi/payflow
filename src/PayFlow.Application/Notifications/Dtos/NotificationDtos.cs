namespace PayFlow.Application.Notifications.Dtos;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    string Type,
    bool IsRead,
    Guid? RelatedEntityId,
    DateTime CreatedAtUtc);

public sealed record MarkAllNotificationsReadResult(int MarkedCount);
