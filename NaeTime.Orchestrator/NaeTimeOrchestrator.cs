using Microsoft.Extensions.DependencyInjection;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Orchestrator;

public class NaeTimeOrchestrator : INaeTimeOrchestrator
{
    public IHardwareOrchestrator Hardware { get; }
    public IManagementOrchestrator Management { get; }
    public IOpenPracticeOrchestrator OpenPractice { get; }
    public ITimingOrchestrator Timing { get; }

    private readonly INaeTimeOrchestratorPersistence _persistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public NaeTimeOrchestrator(INaeTimeOrchestratorPersistence orchestratorPersistence, INaeTimePersistence persistence, INaeTimeOrchestratorDistribution distribution, IServiceProvider serviceProvider)
    {
        _persistence = orchestratorPersistence;
        _distribution = distribution;

        Hardware = ActivatorUtilities.GetServiceOrCreateInstance<HardwareOrchestrator>(serviceProvider);
        Management = ActivatorUtilities.GetServiceOrCreateInstance<ManagementOrchestrator>(serviceProvider);
        OpenPractice = ActivatorUtilities.GetServiceOrCreateInstance<OpenPracticeOrchestrator>(serviceProvider);
        Timing = ActivatorUtilities.GetServiceOrCreateInstance<TimingOrchestrator>(serviceProvider);
    }

    public async Task CommitAsync()
    {
        await _persistence.CommitAsync();
        await _distribution.CommitAsync();
    }
}
