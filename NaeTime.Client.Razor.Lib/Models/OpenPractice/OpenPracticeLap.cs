namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class OpenPracticeLap
{
    public Guid Id { get; set; }
    public Guid PilotId { get; set; }
    public string? PilotName { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime FinishedUtc { get; set; }
    public OpenPracticeLapStatus Status { get; set; }
    public long TotalMilliseconds { get; set; }
}