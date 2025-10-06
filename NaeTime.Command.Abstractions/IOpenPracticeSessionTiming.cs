namespace NaeTime.Command.Abstractions;
public interface IOpenPracticeSessionTiming
{
    public Task AssignDetectionToOpenPracticeSession(Guid detectionId, Guid sessionId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task UnassignDetectionFromOpenPracticeSession(Guid detectionId, Guid sessionId);
}
