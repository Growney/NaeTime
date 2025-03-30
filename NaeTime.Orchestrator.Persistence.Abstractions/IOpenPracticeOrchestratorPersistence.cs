namespace NaeTime.Orchestrator.Persistence.Abstractions;

public interface IOpenPracticeOrchestratorPersistence
{
    public Task ConfigureLaneStatus(Guid sessionId, byte lane, bool isEnabled);
    public Task ConfigureLaneRadioFrequency(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz);
    public Task ConfigureLanePilot(Guid sessionId, byte lane, Guid pilotId);
}
