using System.Security.Cryptography;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Infrastructure.Transfers;

public sealed class ReferenceNumberGenerator(IDateTimeProvider clock) : IReferenceNumberGenerator
{
    public string Next()
    {
        var stamp = clock.UtcNow.ToString("yyyyMMddHHmmss");
        var suffix = Convert.ToHexString(RandomNumberGenerator.GetBytes(6));
        return $"PF-{stamp}-{suffix}";
    }
}
