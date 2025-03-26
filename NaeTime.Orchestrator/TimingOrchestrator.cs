using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;

namespace NaeTime.Orchestrator;

public class TimingOrchestrator : ITimingOrchestrator
{
    private readonly ITimingOrchestratorPersistence _persistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public TimingOrchestrator(ITimingOrchestratorPersistence persistence, INaeTimeOrchestratorDistribution distribution)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _distribution = distribution ?? throw new ArgumentNullException(nameof(distribution));
    }
}
