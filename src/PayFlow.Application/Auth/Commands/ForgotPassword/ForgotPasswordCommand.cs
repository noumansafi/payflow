using MediatR;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;

public sealed record ForgotPasswordResult(string Message, string? ResetToken);

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

        // Avoid account enumeration in the public response.
        if (user is null || !user.IsActive)
        {
            return new ForgotPasswordResult(GenericMessage, null);
        }

        var resetToken = tokenService.CreateRefreshToken();
        user.PasswordResetTokenHash = tokenService.HashToken(resetToken);
        user.PasswordResetTokenExpiresAtUtc = clock.UtcNow.AddHours(1);
        user.UpdatedAtUtc = clock.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailAsync(
            user.Email,
            "Reset your PayFlow password",
            $"Your password reset token is: {resetToken}",
            cancellationToken);

        // Returned for local/demo convenience (mock email). Not a production pattern.
        return new ForgotPasswordResult(GenericMessage, resetToken);
    }
}
