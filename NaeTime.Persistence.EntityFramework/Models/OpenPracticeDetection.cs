namespace NaeTime.Persistence.EntityFramework.Models;
public class OpenPracticeDetection
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid TrackId { get; set; }
    public Guid TimerId { get; set; }
    public int TimerIndex { get; set; }
    public byte Lane { get; set; }
    public Guid? PilotId { get; set; }
    public ulong? HardwareTime { get; set; }
    public long SoftwareTime { get; set; }
    public DateTime UtcTime { get; set; }
}
