using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Notifications.Commands.MarkNotificationRead;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.Notifications;

public sealed class MarkNotificationReadCommandHandlerTests
{
    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private MarkNotificationReadCommandHandler CreateSut() =>
        new(_notifications, _currentUser, _unitOfWork);

    [Fact]
    public async Task Handle_WhenUnread_MarksReadAndSaves()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId, isRead: false);

        _currentUser.UserId.Returns(userId);
        _notifications.GetByIdForUserAsync(notification.Id, userId, Arg.Any<CancellationToken>())
            .Returns(notification);

        await CreateSut().Handle(new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        notification.IsRead.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyRead_IsIdempotent()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId, isRead: true);

        _currentUser.UserId.Returns(userId);
        _notifications.GetByIdForUserAsync(notification.Id, userId, Arg.Any<CancellationToken>())
            .Returns(notification);

        await CreateSut().Handle(new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        notification.IsRead.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMissingOrForeign_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _notifications.GetByIdForUserAsync(id, userId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var act = () => CreateSut().Handle(new MarkNotificationReadCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(
            new MarkNotificationReadCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    private static Notification CreateNotification(Guid userId, bool isRead) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Transfer received",
            Body = "You received 10.00 USD",
            Type = NotificationType.TransferReceived,
            IsRead = isRead,
            CreatedAtUtc = DateTime.UtcNow
        };
}
