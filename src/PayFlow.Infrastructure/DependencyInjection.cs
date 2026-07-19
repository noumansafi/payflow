using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayFlow.Infrastructure.Persistence;

namespace PayFlow.Infrastructure;

/// <summary>
/// Infrastructure composition root.
/// Registers EF Core, SQL Server, auth stores, and external integrations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<PayFlowDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
