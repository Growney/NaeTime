namespace NaeTime.Timing.Abstractions.Models;
public class RssiLevel
{
    public Guid Id { get; }
    public ulong? HardwareTime { get; }
    public long SoftwareTime { get; }
    public DateTime DetectionTime { get; }
    public Guid TimerId { get; }
    public byte Channel { get; }
    public float Level { get; }
}
