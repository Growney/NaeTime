using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Orchestrator.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkOrchestratorPersistence(this IServiceCollection services)
    {
        services.AddScoped<IHardwareOrchestratorPersistence, HardwareOrchestratorPersistence>();
        services.AddScoped<IManagementOrchestratorPersistence, ManagementOrchestratorPersistence>();
        services.AddScoped<IOpenPracticeOrchestratorPersistence, OpenPracticeOrchestratorPersistence>();
        services.AddScoped<ITimingOrchestratorPersistence, TimingOrchestratorPersistence>();
        services.AddScoped<INaeTimeOrchestratorPersistence, NaeTimeOrchestratorPersistence>();

        return services;
    }
}
