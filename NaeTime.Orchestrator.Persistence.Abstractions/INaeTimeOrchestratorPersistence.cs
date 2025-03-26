namespace NaeTime.Orchestrator.Persistence.Abstractions;

public interface INaeTimeOrchestratorPersistence
{
    public IHardwareOrchestratorPersistence Hardware { get; }
    public IManagementOrchestratorPersistence Management { get; }
    public IOpenPracticeOrchestratorPersistence OpenPractice { get; }
    public ITimingOrchestratorPersistence Timing { get; }

    public Task CommitAsync();
}
