namespace NaeTime.Command.Abstractions;
public interface IDetectionCommandHandler
{
    public Task RegisterHardwareDetection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);

    public Task AssignDetectionToOpenPracticeSesssion(Guid detectionId, Guid sessionId);
}
