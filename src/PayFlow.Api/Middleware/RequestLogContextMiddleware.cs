using System.Diagnostics;
using System.Security.Claims;
using Serilog.Context;

namespace PayFlow.Api.Middleware;

/// <summary>
/// Pushes per-request properties into Serilog's <see cref="LogContext"/>
/// so all <c>ILogger</c> calls during the request include TraceId / UserId.
/// </summary>
public sealed class RequestLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        using (LogContext.PushProperty("UserId", userId ?? "(anonymous)"))
        using (LogContext.PushProperty("RequestId", Activity.Current?.Id ?? context.TraceIdentifier))
        {
            await next(context);
        }
    }
}
