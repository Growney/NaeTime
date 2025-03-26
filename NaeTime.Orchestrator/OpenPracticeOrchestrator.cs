using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;

namespace NaeTime.Orchestrator;

public class OpenPracticeOrchestrator : IOpenPracticeOrchestrator
{
    private readonly IOpenPracticeOrchestratorPersistence _persistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public OpenPracticeOrchestrator(IOpenPracticeOrchestratorPersistence persistence, INaeTimeOrchestratorDistribution distribution)
    {
        _persistence = persistence;
        _distribution = distribution;
    }
}
