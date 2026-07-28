using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Common.Models;
using PayFlow.Application.Notifications.Dtos;

namespace PayFlow.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 20,
    bool? IsRead = null) : IRequest<PagedResult<NotificationDto>>;

public sealed class GetNotificationsQueryHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser) : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<PagedResult<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var skip = (request.Page - 1) * request.PageSize;
        var result = await notifications.ListForUserAsync(
            userId,
            request.IsRead,
            skip,
            request.PageSize,
            cancellationToken);

        var items = result.Items.Select(NotificationMapping.ToDto).ToList();

        return new PagedResult<NotificationDto>(items, request.Page, request.PageSize, result.TotalCount);
    }
}
