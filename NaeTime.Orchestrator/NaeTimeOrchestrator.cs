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

    public NaeTimeOrchestrator(INaeTimeOrchestratorPersistence orchestratorPersistence, INaeTimePersistence persistence, INaeTimeOrchestratorDistribution distribution)
    {
        _persistence = orchestratorPersistence;
        _distribution = distribution;

        Hardware = new HardwareOrchestrator(orchestratorPersistence.Hardware, distribution);
        Management = new ManagementOrchestrator(orchestratorPersistence.Management, persistence, distribution);
        OpenPractice = new OpenPracticeOrchestrator(orchestratorPersistence.OpenPractice, distribution);
        Timing = new TimingOrchestrator(orchestratorPersistence.Timing, distribution);
    }

    public async Task CommitAsync()
    {
        await _persistence.CommitAsync();
        await _distribution.CommitAsync();
    }
}
