namespace NaeTime.Orchestrator.Abstractions;

public interface INaeTimeOrchestrator
{
    public IHardwareOrchestrator Hardware { get; }
    public IManagementOrchestrator Management { get; }
    public IOpenPracticeOrchestrator OpenPractice { get; }
    public ITimingOrchestrator Timing { get; }

    public Task CommitAsync();
}
