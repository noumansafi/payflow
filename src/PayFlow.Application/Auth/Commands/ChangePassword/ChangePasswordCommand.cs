using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest;

public sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    IAuditLogger auditLogger,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAppException();
        }

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAppException();

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException();
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            throw new UnauthorizedAppException("Current password is incorrect.");
        }

        var now = clock.UtcNow;
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatedAtUtc = now;

        // Invalidate existing sessions after a password change.
        await refreshTokens.RevokeAllActiveForUserAsync(user.Id, now, cancellationToken);

        await auditLogger.WriteAsync(
            AuditAction.PasswordChange,
            "User",
            user.Id,
            user.Id,
            """{"event":"password_changed"}""",
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
