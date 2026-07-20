using Microsoft.Extensions.Logging;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Infrastructure.Auth;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Mock email to {ToEmail} | Subject: {Subject} | Body: {Body}",
            toEmail,
            subject,
            body);

        return Task.CompletedTask;
    }
}
