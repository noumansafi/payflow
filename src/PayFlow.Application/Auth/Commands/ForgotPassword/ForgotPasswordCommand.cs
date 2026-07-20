using MediatR;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;

public sealed record ForgotPasswordResult(string Message);

public sealed class ForgotPasswordCommandHandler(
    IUserRepository users,
    ITokenService tokenService,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private const string GenericMessage =
        "If an account exists for that email, a password reset token has been issued.";

    public async Task<ForgotPasswordResult> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, cancellationToken);

        // Same response whether or not the account exists (no enumeration via body fields).
        if (user is null || !user.IsActive)
        {
            return new ForgotPasswordResult(GenericMessage);
        }

        var resetToken = tokenService.CreateRefreshToken();
        user.PasswordResetTokenHash = tokenService.HashToken(resetToken);
        user.PasswordResetTokenExpiresAtUtc = clock.UtcNow.AddHours(1);
        user.UpdatedAtUtc = clock.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Token is delivered only via mock email/logs — never in the HTTP response.
        await emailSender.SendEmailAsync(
            user.Email,
            "Reset your PayFlow password",
            $"Your password reset token is: {resetToken}",
            cancellationToken);

        return new ForgotPasswordResult(GenericMessage);
    }
}
