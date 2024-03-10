namespace NaeTime.Persistence.SQLite.Models;
public class Detection
{
    public Guid Id { get; set; }
    public Guid TimerId { get; set; }
    public byte Lane { get; set; }
    public ulong? HardwareTime { get; set; }
    public long SoftwareTime { get; set; }
    public DateTime UtcTime { get; set; }
    public bool IsManual { get; set; }
}
