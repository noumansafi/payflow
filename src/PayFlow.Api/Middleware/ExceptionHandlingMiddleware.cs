using System.Text.Json;
using PayFlow.Application.Common.Exceptions;

namespace PayFlow.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await WriteProblemAsync(context, exception);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationAppException validation => (
                validation.StatusCode,
                validation.Title,
                validation.Message,
                (IDictionary<string, string[]>?)validation.Errors),
            AppException appException => (
                appException.StatusCode,
                appException.Title,
                appException.Message,
                null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server Error",
                "An unexpected error occurred.",
                null)
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception for {RequestMethod} {RequestPath}. TraceId {TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Handled {ErrorTitle} ({StatusCode}) for {RequestMethod} {RequestPath}. TraceId {TraceId}",
                title,
                statusCode,
                context.Request.Method,
                context.Request.Path.Value,
                context.TraceIdentifier);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.com/{statusCode}",
            ["title"] = title,
            ["status"] = statusCode,
            ["detail"] = detail,
            ["traceId"] = context.TraceIdentifier
        };

        if (errors is not null)
        {
            problem["errors"] = errors;
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, SerializerOptions),
            context.RequestAborted);
    }
}
