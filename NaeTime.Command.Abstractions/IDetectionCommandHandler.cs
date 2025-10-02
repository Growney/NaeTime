namespace NaeTime.Command.Abstractions;
public interface IDetectionCommandHandler
{
    public Task RegisterHardwareDetection(Guid DetectionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
    public Task RegisterDetection(Guid DetectionId, byte Lane, long SoftwareTime, DateTime UtcTime);

    public Task AssignDetectionToSession(Guid DetectionId, Guid SessionId);
    public Task UnassignDetectionFromSession(Guid DetectionId, Guid SessionId);
}
