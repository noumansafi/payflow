using Microsoft.Extensions.DependencyInjection;

namespace PayFlow.Application;

/// <summary>
/// Application-layer composition root.
/// Registers MediatR, FluentValidation, and application services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Milestone 2+: MediatR, FluentValidation, pipeline behaviors
        return services;
    }
}
