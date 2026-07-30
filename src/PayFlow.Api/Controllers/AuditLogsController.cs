using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.Application.AuditLogs.Dtos;
using PayFlow.Application.AuditLogs.Queries.GetAuditLogs;
using PayFlow.Application.Common.Models;
using PayFlow.Domain.Enums;

namespace PayFlow.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/v1/admin/audit-logs")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class AuditLogsController(ISender sender) : ControllerBase
{
    /// <summary>List audit logs (administrators only).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] AuditAction? action = null,
        [FromQuery] Guid? actorUserId = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetAuditLogsQuery(page, pageSize, action, actorUserId, fromUtc, toUtc),
            cancellationToken);

        return Ok(result);
    }
}
