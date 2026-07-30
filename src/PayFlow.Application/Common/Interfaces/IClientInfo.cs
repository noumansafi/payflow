namespace PayFlow.Application.Common.Interfaces;

/// <summary>
/// Request-scoped client metadata from the HTTP edge.
/// Implementations must never expose secrets.
/// </summary>
public interface IClientInfo
{
    string? IpAddress { get; }
}
