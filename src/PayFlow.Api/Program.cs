using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PayFlow.Api.Middleware;
using PayFlow.Api.OpenApi;
using PayFlow.Api.Services;
using PayFlow.Application;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Application.Options;
using PayFlow.Infrastructure;
using PayFlow.Infrastructure.Persistence;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting PayFlow API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddPayFlowSwagger();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing.");

    if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
    {
        throw new InvalidOperationException("Jwt:Secret must be configured and at least 32 characters.");
    }

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, _, exception) =>
            exception is not null || httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
            diagnosticContext.Set(
                "UserId",
                httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "(anonymous)");
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set(
                "ClientIp",
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "(unknown)");
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UsePayFlowSwagger();
        await MigrateDatabaseWithRetryAsync(app.Services);
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<RequestLogContextMiddleware>();
    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "PayFlow API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static async Task MigrateDatabaseWithRetryAsync(IServiceProvider services)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PayFlowDbContext>();
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            Log.Warning(
                ex,
                "Database not ready (attempt {Attempt}/{MaxAttempts})",
                attempt,
                maxAttempts);
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not connect to SQL Server to apply migrations");
            throw new InvalidOperationException(
                """
                Could not connect to SQL Server to apply migrations.

                Start SQL Server first, then rerun the API:
                  docker compose up sqlserver -d

                Default local connection string expects:
                  Server=localhost,1433
                  User Id=sa
                  Password=PayFlow_Strong_Passw0rd
                """,
                ex);
        }
    }
}

public partial class Program;
