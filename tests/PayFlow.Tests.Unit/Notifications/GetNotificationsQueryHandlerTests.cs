using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Notifications.Queries.GetNotifications;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Notifications;

public sealed class GetNotificationsQueryHandlerTests
{
    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetNotificationsQueryHandler CreateSut() =>
        new(_notifications, _currentUser);

    [Fact]
    public async Task Handle_WhenOwner_ReturnsPagedDtos()
    {
        var userId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Transfer received",
            Body = "You received 10.00 USD",
            Type = NotificationType.TransferReceived,
            IsRead = false,
            RelatedEntityId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _currentUser.UserId.Returns(userId);
        _notifications.ListForUserAsync(userId, null, 0, 20, Arg.Any<CancellationToken>())
            .Returns(new NotificationListResult([notification], 1));

        var result = await CreateSut().Handle(new GetNotificationsQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Transfer received");
        result.Items[0].Type.Should().Be(nameof(NotificationType.TransferReceived));
        result.Items[0].IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenUnreadFilter_PassesFilterToRepository()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _notifications.ListForUserAsync(userId, false, 0, 20, Arg.Any<CancellationToken>())
            .Returns(new NotificationListResult([], 0));

        await CreateSut().Handle(new GetNotificationsQuery(IsRead: false), CancellationToken.None);

        await _notifications.Received(1).ListForUserAsync(
            userId,
            false,
            0,
            20,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new GetNotificationsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }
}
