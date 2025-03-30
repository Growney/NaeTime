using NaeTime.Orchestrator;
using NaeTime.Orchestrator.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeOrchestrator(this IServiceCollection services)
    {
        services.AddScoped<INaeTimeOrchestrator, NaeTimeOrchestrator>();
        return services;
    }
}
