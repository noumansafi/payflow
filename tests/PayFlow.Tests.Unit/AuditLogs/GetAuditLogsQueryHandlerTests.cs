using FluentAssertions;
using NSubstitute;
using PayFlow.Application.AuditLogs.Queries.GetAuditLogs;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Tests.Unit.AuditLogs;

public sealed class GetAuditLogsQueryHandlerTests
{
    private readonly IAuditLogRepository _auditLogs = Substitute.For<IAuditLogRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetAuditLogsQueryHandler CreateSut() =>
        new(_auditLogs, _currentUser);

    [Fact]
    public async Task Handle_WhenAdmin_ReturnsPagedDtos()
    {
        var adminId = Guid.NewGuid();
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = adminId,
            Action = AuditAction.Login,
            EntityType = "User",
            EntityId = adminId,
            Metadata = """{"event":"login_success"}""",
            IpAddress = "127.0.0.1",
            CreatedAtUtc = DateTime.UtcNow
        };

        _currentUser.UserId.Returns(adminId);
        _currentUser.Role.Returns(nameof(UserRole.Admin));
        _auditLogs.ListAsync(null, null, null, null, 0, 20, Arg.Any<CancellationToken>())
            .Returns(new AuditLogListResult([log], 1));

        var result = await CreateSut().Handle(new GetAuditLogsQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Action.Should().Be(nameof(AuditAction.Login));
        result.Items[0].IpAddress.Should().Be("127.0.0.1");
    }

    [Fact]
    public async Task Handle_WhenFiltersProvided_PassesFiltersToRepository()
    {
        var adminId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        _currentUser.UserId.Returns(adminId);
        _currentUser.Role.Returns(nameof(UserRole.Admin));
        _auditLogs.ListAsync(
                AuditAction.Transfer,
                actorId,
                from,
                to,
                20,
                20,
                Arg.Any<CancellationToken>())
            .Returns(new AuditLogListResult([], 0));

        await CreateSut().Handle(
            new GetAuditLogsQuery(
                Page: 2,
                PageSize: 20,
                Action: AuditAction.Transfer,
                ActorUserId: actorId,
                FromUtc: from,
                ToUtc: to),
            CancellationToken.None);

        await _auditLogs.Received(1).ListAsync(
            AuditAction.Transfer,
            actorId,
            from,
            to,
            20,
            20,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNonAdmin_ThrowsForbidden()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.Role.Returns(nameof(UserRole.User));

        var act = () => CreateSut().Handle(new GetAuditLogsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _auditLogs.DidNotReceive().ListAsync(
            Arg.Any<AuditAction?>(),
            Arg.Any<Guid?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new GetAuditLogsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }
}
