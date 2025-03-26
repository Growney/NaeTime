namespace NaeTime.Orchestrator.Persistence.Abstractions;

public interface IHardwareOrchestratorPersistence
{
    Task DisconnectTimer(Guid timerId, DateTime disconnectionTime);
    Task ConnectTimer(Guid timerId, DateTime connectionTime);
}
