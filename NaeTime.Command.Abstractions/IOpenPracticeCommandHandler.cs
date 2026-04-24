namespace NaeTime.Command.Abstractions;
public interface IOpenPracticeCommandHandler
{
    public Task ScheduleSession(Guid id, Guid trackId, string name, TimeSpan? minimumLapTime, TimeSpan? maximumLapTime);
    public Task CloneSession(Guid newId, Guid existingId, string newName);
    public Task CloneSessionOnNewTrack(Guid newId, Guid existingId, string newName, Guid trackId);
    public Task RenameSession(Guid sessionId, string name);
    public Task DisableLane(Guid sessionId, byte lane);
    public Task EnableLane(Guid sessionId, byte lane);
    public Task TuneLane(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz);
    public Task SetLanePilot(Guid sessionId, byte lane, Guid pilotId);
    public Task ResetLanePilot(Guid sessionId, byte lane);

    public Task AssignHardwareDetectionToSession(Guid detectionId, Guid sessionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task TriggerDetection(Guid detectionId, Guid sessionId, byte lane, byte ordinalPosition);
    public Task InvalidateDetection(Guid sessionId, Guid detectionId);
    public Task InvalidateAllPilotDetections(Guid sessionId, Guid pilotId);
    public Task InvalidatePilotDetectionsBeforeDetection(Guid sessionId, Guid detectionId);
    public Task ValidateDetection(Guid detectionId, Guid sessionId);
    public Task InsertPilotPackEndBeforeDetection(Guid sessionId, Guid detectionId);
    public Task InsertPilotPackEndAfterDetection(Guid sessionId, Guid detectionId);
    public Task RemovePilotPackEnd(Guid sessionId, Guid packEndId);
    public Task SetMinimumLapTime(Guid sessionId, TimeSpan minimumLapTime);
    public Task ResetMinimumLapTime(Guid sessionId);
    public Task SetMaximumLapTime(Guid sessionId, TimeSpan maximumLapTime);
    public Task ResetMaximumLapTime(Guid sessionId);

}
