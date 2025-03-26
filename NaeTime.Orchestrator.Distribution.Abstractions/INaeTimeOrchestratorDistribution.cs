namespace NaeTime.Orchestrator.Distribution.Abstractions;

public interface INaeTimeOrchestratorDistribution
{
    public Task Distribute<T>(T message);
    public Task CommitAsync();
}
