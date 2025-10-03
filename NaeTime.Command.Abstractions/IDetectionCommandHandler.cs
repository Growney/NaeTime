namespace NaeTime.Command.Abstractions;
public interface IDetectionCommandHandler
{
    public Task RegisterHardwareDetection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task RegisterDetection(Guid detectionId, byte lane, long softwareTime, DateTime utcTime);

    public Task AssignDetectionToOpenPracticeSession(Guid detectionId, Guid sessionId);
    public Task UnassignDetectionFromSession(Guid detectionId, Guid sessionId);
}
