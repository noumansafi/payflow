using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Api.Services;

public sealed class ClientInfo(IHttpContextAccessor httpContextAccessor) : IClientInfo
{
    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
