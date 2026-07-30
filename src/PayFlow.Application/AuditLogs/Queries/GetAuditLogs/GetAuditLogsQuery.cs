using MediatR;
using PayFlow.Application.AuditLogs.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Common.Models;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 20,
    AuditAction? Action = null,
    Guid? ActorUserId = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IRequest<PagedResult<AuditLogDto>>;

public sealed class GetAuditLogsQueryHandler(
    IAuditLogRepository auditLogs,
    ICurrentUser currentUser) : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public async Task<PagedResult<AuditLogDto>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAppException();
        }

        if (!string.Equals(currentUser.Role, nameof(UserRole.Admin), StringComparison.Ordinal))
        {
            throw new ForbiddenException("Only administrators can list audit logs.");
        }

        var skip = (request.Page - 1) * request.PageSize;
        var result = await auditLogs.ListAsync(
            request.Action,
            request.ActorUserId,
            request.FromUtc,
            request.ToUtc,
            skip,
            request.PageSize,
            cancellationToken);

        var items = result.Items.Select(AuditLogMapping.ToDto).ToList();

        return new PagedResult<AuditLogDto>(items, request.Page, request.PageSize, result.TotalCount);
    }
}
