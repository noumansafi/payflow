using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
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
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var now = clock.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var existing = await refreshTokens.GetByHashAsync(
                tokenService.HashToken(request.RefreshToken),
                cancellationToken);

            // Only revoke tokens that belong to the authenticated user.
            if (existing is not null
                && existing.UserId == userId
                && existing.RevokedAtUtc is null)
            {
                existing.RevokedAtUtc = now;
            }
        }
        else
        {
            // No specific token provided — end all sessions for this user.
            await refreshTokens.RevokeAllActiveForUserAsync(userId, now, cancellationToken);
        }

        await auditLogger.WriteAsync(
            AuditAction.Logout,
            "User",
            userId,
            userId,
            """{"event":"logout"}""",
            request.IpAddress,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
