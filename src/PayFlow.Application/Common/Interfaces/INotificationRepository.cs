using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface INotificationRepository
{
    void Add(Notification notification);

    Task<NotificationListResult> ListForUserAsync(
        Guid userId,
        bool? isRead,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}

public sealed record NotificationListResult(
    IReadOnlyList<Notification> Items,
    int TotalCount);
