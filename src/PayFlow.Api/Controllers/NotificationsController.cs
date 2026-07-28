using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.Application.Common.Models;
using PayFlow.Application.Notifications.Commands.MarkAllNotificationsRead;
using PayFlow.Application.Notifications.Commands.MarkNotificationRead;
using PayFlow.Application.Notifications.Dtos;
using PayFlow.Application.Notifications.Queries.GetNotifications;

namespace PayFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    /// <summary>List notifications for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetNotificationsQuery(page, pageSize, isRead),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Mark all unread notifications as read for the authenticated user.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(MarkAllNotificationsReadResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var markedCount = await sender.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return Ok(new MarkAllNotificationsReadResult(markedCount));
    }

    /// <summary>Mark a single notification as read (owner only; idempotent).</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return NoContent();
    }
}
