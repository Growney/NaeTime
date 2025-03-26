using NaeTime.Orchestrator;
using NaeTime.Orchestrator.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeOrchestrator(this IServiceCollection services)
    {
        services.AddScoped<IHardwareOrchestrator, HardwareOrchestrator>();
        services.AddScoped<IManagementOrchestrator, ManagementOrchestrator>();
        services.AddScoped<INaeTimeOrchestrator, NaeTimeOrchestrator>();
        services.AddScoped<IOpenPracticeOrchestrator, OpenPracticeOrchestrator>();
        services.AddScoped<ITimingOrchestrator, TimingOrchestrator>();
        return services;
    }
}
