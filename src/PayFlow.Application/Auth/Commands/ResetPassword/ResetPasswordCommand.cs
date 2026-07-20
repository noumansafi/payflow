using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword) : IRequest;

public sealed class ResetPasswordCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IAuditLogger auditLogger,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenService.HashToken(request.Token);
        var user = await users.GetByPasswordResetTokenHashAsync(tokenHash, cancellationToken);

        if (user is null
            || !user.IsActive
            || user.PasswordResetTokenExpiresAtUtc is null
            || user.PasswordResetTokenExpiresAtUtc <= now)
        {
            throw new UnauthorizedAppException("Invalid or expired password reset token.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAtUtc = null;
        user.UpdatedAtUtc = now;

        await refreshTokens.RevokeAllActiveForUserAsync(user.Id, now, cancellationToken);

        await auditLogger.WriteAsync(
            AuditAction.PasswordChange,
            "User",
            user.Id,
            user.Id,
            """{"event":"password_reset"}""",
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
