using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Application.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest;

public sealed class MarkNotificationReadCommandHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var notification = await notifications.GetByIdForUserAsync(
                request.NotificationId,
                userId,
                cancellationToken)
            ?? throw new NotFoundException("Notification was not found.");

        if (notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
