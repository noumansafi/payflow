using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Infrastructure.Persistence;

/// <summary>Fallback when no HTTP request is available (e.g. background work).</summary>
internal sealed class NullClientInfo : IClientInfo
{
    public string? IpAddress => null;
}
