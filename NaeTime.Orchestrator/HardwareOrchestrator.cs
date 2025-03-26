using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;
using NaeTime.Orchestrator.Persistence.Abstractions;

namespace NaeTime.Orchestrator;

public class HardwareOrchestrator : IHardwareOrchestrator
{
    private readonly IHardwareOrchestratorPersistence _persistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public HardwareOrchestrator(IHardwareOrchestratorPersistence persistence, INaeTimeOrchestratorDistribution distribution)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _distribution = distribution ?? throw new ArgumentNullException(nameof(distribution));
    }

    public async Task ConnectTimer(Guid timerId, DateTime connectionTime)
    {
        await _persistence.ConnectTimer(timerId, connectionTime);

        await _distribution.Distribute(new TimerConnected(timerId));
    }

    public async Task DisconnectTimer(Guid timerId, DateTime disconnectionTime)
    {
        await _persistence.DisconnectTimer(timerId, disconnectionTime);

        await _distribution.Distribute(new TimerDisconnected(timerId));
    }
}
