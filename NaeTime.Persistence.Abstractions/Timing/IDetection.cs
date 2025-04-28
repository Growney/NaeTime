namespace NaeTime.Persistence.Abstractions.Timing;
public interface IDetection
{
    public Guid TimerId { get; }
    public ulong? HardwareTime { get; }
    public long SoftwareTime { get; }
    public DateTime UtcTime { get; }

}
