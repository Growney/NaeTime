namespace NaeTime.Persistence.EntityFramework.Models;
public class OpenPracticeLap
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid PilotId { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime FinishedUtc { get; set; }
    public long TotalMilliseconds { get; set; }
    public Guid StartDetection { get; set; }
    public Guid EndDetection { get; set; }
}