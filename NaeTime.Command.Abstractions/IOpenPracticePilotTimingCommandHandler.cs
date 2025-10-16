namespace NaeTime.Command.Abstractions;
public interface IOpenPracticePilotTimingCommandHandler
{

    public Task StartPilotTimingSession(Guid SessionId, Guid PilotId);
    public Task AddDetectionOccurance(Guid DetectionId, Guid SessionId, Guid PilotId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
    public Task TriggerDetection(Guid DetectionId, Guid SessionId, Guid PilotId, byte lane, byte OrdinalPosition, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
}
