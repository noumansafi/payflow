using Microsoft.EntityFrameworkCore;
using PayFlow.Application;
using PayFlow.Infrastructure;
using PayFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await MigrateDatabaseWithRetryAsync(app.Services);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task MigrateDatabaseWithRetryAsync(IServiceProvider services)
{
    // Convenient for local/Docker demos. Production should migrate via CI/CD or a release job.
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PayFlowDbContext>();
            await dbContext.Database.MigrateAsync();
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            Console.WriteLine(
                $"[PayFlow] Database not ready (attempt {attempt}/{maxAttempts}): {ex.Message}");
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
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

// Required for WebApplicationFactory integration tests
public partial class Program;
