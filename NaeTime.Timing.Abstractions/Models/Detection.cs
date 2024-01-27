namespace NaeTime.Timing.Abstractions.Models;
public class Detection
{
    public Detection(Guid id, ulong hardwareTime, long softwareTime, DateTime detectionTime, Guid timerId, byte channel)
    {
        Id = id;
        HardwareTime = hardwareTime;
        SoftwareTime = softwareTime;
        DetectionTime = detectionTime;
        TimerId = timerId;
        Channel = channel;
    }

    public Guid Id { get; }
    public ulong HardwareTime { get; }
    public long SoftwareTime { get; }
    public DateTime DetectionTime { get; }
    public Guid TimerId { get; }
    public byte Channel { get; }
}
