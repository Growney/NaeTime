namespace NaeTime.Orchestrator.Abstractions;

public interface IHardwareOrchestrator
{
    public Task DisconnectTimer(Guid timerId, DateTime disconnectionTime);
    public Task ConnectTimer(Guid timerId, DateTime connectionTime);
}
