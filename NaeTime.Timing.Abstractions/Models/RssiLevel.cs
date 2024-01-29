namespace NaeTime.Timing.Abstractions.Models;
public class RssiLevel
{
    public RssiLevel(Guid id, ulong? hardwareTime, long softwareTime, DateTime detectionTime, Guid timerId, byte channel, float level)
    {
        Id = id;
        HardwareTime = hardwareTime;
        SoftwareTime = softwareTime;
        DetectionTime = detectionTime;
        TimerId = timerId;
        Channel = channel;
        Level = level;
    }

    public Guid Id { get; }
    public ulong? HardwareTime { get; }
    public long SoftwareTime { get; }
    public DateTime DetectionTime { get; }
    public Guid TimerId { get; }
    public byte Channel { get; }
    public float Level { get; }
}
