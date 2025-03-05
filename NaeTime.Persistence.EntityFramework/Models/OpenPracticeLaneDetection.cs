namespace NaeTime.Persistence.EntityFramework.Models;
public class OpenPracticeLaneDetection
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid DetectionId { get; set; }
    public Guid? TimerId { get; set; }
    public byte TimerIndex { get; set; }
    public byte Lane { get; set; }
    public ulong? HardwareTime { get; set; }
    public long SoftwareTime { get; set; }
    public DateTime UtcTime { get; set; }
}