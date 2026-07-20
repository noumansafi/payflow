using MediatR;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : IRequest;

public sealed class VerifyEmailCommandHandler(
    IUserRepository users,
    ITokenService tokenService,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<VerifyEmailCommand>
{
    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenService.HashToken(request.Token);
        var user = await users.GetByEmailVerificationTokenHashAsync(tokenHash, cancellationToken);

        if (user is null
            || user.EmailVerificationTokenExpiresAtUtc is null
            || user.EmailVerificationTokenExpiresAtUtc <= now)
        {
            throw new UnauthorizedAppException("Invalid or expired email verification token.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAtUtc = null;
        user.UpdatedAtUtc = now;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
