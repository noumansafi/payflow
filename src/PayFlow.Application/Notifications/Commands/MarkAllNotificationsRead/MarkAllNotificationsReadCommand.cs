using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Application.Notifications.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand : IRequest<int>;

public sealed class MarkAllNotificationsReadCommandHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser) : IRequestHandler<MarkAllNotificationsReadCommand, int>
{
    public async Task<int> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        return await notifications.MarkAllUnreadAsReadAsync(userId, cancellationToken);
    }
}
