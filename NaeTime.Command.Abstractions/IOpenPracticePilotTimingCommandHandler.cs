namespace NaeTime.Command.Abstractions;
public interface IOpenPracticePilotTimingCommandHandler
{
    public Task AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task RemoveDetectionFromPilot(Guid pilotId, Guid sessionId, Guid detectionId);
    public Task MarkPilotDetectionAsValid(Guid pilotId, Guid sessionId, Guid detectionId);
    public Task MarkPilotDetectionAsInvalid(Guid pilotId, Guid sessionId, Guid detectionId);
}
