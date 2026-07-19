using Microsoft.Extensions.DependencyInjection;

namespace PayFlow.Infrastructure;

/// <summary>
/// Infrastructure composition root.
/// Registers EF Core, SQL Server, auth stores, and external integrations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Milestone 1 (next): DbContext, SQL Server, repositories
        return services;
    }
}
