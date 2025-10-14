namespace NaeTime.Command.Abstractions;
public interface IOpenPracticeCommandHandler
{
    public Task ScheduleSession(Guid id, Guid trackId, string name);
    public Task CloneSession(Guid newId, Guid existingId, string newName);
    public Task CloneSessionOnNewTrack(Guid newId, Guid existingId, string newName, Guid trackId);
    public Task RenameSession(Guid sessionId, string name);
    public Task DisableLane(Guid sessionId, byte lane);
    public Task EnableLane(Guid sessionId, byte lane);
    public Task TuneLane(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz);
    public Task SetLanePilot(Guid sessionId, byte lane, Guid pilotId);
    public Task ResetLanePilot(Guid sessionId, byte lane);

    public Task AddDetectionToSession(Guid detectionId, Guid sessionId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task RemoveDetectionFromSession(Guid detectionId, Guid sessionId);
    public Task AssignDetectionToPilot(Guid detectionId, Guid sessionId, Guid pilotId);
    public Task UnassignDetectionFromPilot(Guid detectionId,Guid sessionId, Guid pilotId);
}
