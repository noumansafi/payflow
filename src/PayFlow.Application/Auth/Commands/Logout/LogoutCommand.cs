using MediatR;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(
    string? RefreshToken,
    string? IpAddress = null) : IRequest;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    ITokenService tokenService,
    IAuditLogger auditLogger,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var existing = await refreshTokens.GetByHashAsync(
                tokenService.HashToken(request.RefreshToken),
                cancellationToken);

            if (existing is not null && existing.RevokedAtUtc is null)
            {
                existing.RevokedAtUtc = clock.UtcNow;
            }
        }

        if (currentUser.UserId is Guid userId)
        {
            await auditLogger.WriteAsync(
                AuditAction.Logout,
                "User",
                userId,
                userId,
                """{"event":"logout"}""",
                request.IpAddress,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
