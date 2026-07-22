using PayFlow.Domain.Entities;

namespace PayFlow.Application.Common.Interfaces;

public interface INotificationRepository
{
    void Add(Notification notification);
}
