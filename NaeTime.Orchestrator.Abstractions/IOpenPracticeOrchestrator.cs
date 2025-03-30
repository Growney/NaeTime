namespace NaeTime.Orchestrator.Abstractions;

public interface IOpenPracticeOrchestrator
{
    public Task ConfigureLaneStatus(Guid sessionId, byte lane, bool isEnabled);
    public Task ConfigureLaneRadioFrequency(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz);
    public Task ConfigureLanePilot(Guid sessionId, byte lane, Guid pilotId);
}
