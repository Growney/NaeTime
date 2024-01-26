namespace NaeTime.Timing.Abstractions.Models;
public class Detection
{
    public Detection(Guid id, long hardwareTime, long softwareTime, DateTime detectionTime, Guid timerId, int channel)
    {
        Id = id;
        HardwareTime = hardwareTime;
        SoftwareTime = softwareTime;
        DetectionTime = detectionTime;
        TimerId = timerId;
        Channel = channel;
    }

    public Guid Id { get; }
    public long HardwareTime { get; }
    public long SoftwareTime { get; }
    public DateTime DetectionTime { get; }
    public Guid TimerId { get; }
    public int Channel { get; }
}
