using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(PayFlowDbContext dbContext) : INotificationRepository
{
    public void Add(Notification notification) => dbContext.Notifications.Add(notification);

    public async Task<NotificationListResult> ListForUserAsync(
        Guid userId,
        bool? isRead,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (isRead is { } readFilter)
        {
            query = query.Where(x => x.IsRead == readFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new NotificationListResult(items, totalCount);
    }
}
