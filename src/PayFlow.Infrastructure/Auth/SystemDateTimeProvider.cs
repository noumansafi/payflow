using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Infrastructure.Auth;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
