using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Notifications.Commands.MarkAllNotificationsRead;

namespace PayFlow.Tests.Unit.Notifications;

public sealed class MarkAllNotificationsReadCommandHandlerTests
{
    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private MarkAllNotificationsReadCommandHandler CreateSut() =>
        new(_notifications, _currentUser);

    [Fact]
    public async Task Handle_WhenOwner_MarksAllUnreadAndReturnsCount()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _notifications.MarkAllUnreadAsReadAsync(userId, Arg.Any<CancellationToken>())
            .Returns(3);

        var result = await CreateSut().Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        result.Should().Be(3);
        await _notifications.Received(1).MarkAllUnreadAsReadAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
        await _notifications.DidNotReceive().MarkAllUnreadAsReadAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }
}
