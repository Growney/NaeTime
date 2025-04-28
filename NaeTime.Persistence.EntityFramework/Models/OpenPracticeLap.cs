namespace NaeTime.Persistence.EntityFramework.Models;
public class OpenPracticeLap
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid PilotId { get; set; }
    public Guid EntryDetectionId { get; set; }
    public Guid? ExitDetectionId { get; set; }
    public OpenPracticeLapStatus Status { get; set; }
}