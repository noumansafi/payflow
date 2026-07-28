using PayFlow.Application.Notifications.Dtos;
using PayFlow.Domain.Entities;

namespace PayFlow.Application.Notifications;

internal static class NotificationMapping
{
    public static NotificationDto ToDto(Notification notification) =>
        new(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.Type.ToString(),
            notification.IsRead,
            notification.RelatedEntityId,
            notification.CreatedAtUtc);
}
