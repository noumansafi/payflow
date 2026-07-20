namespace PayFlow.Application.Common.Interfaces;

/// <summary>
/// Mock-friendly email port. Infrastructure logs tokens in Development.
/// </summary>
public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
