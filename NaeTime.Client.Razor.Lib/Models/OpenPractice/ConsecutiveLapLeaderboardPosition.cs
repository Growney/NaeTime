namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class ConsecutiveLapLeadboardPosition
{
    public int Position { get; set; }
    public Guid PilotId { get; set; }
    public uint TotalLaps { get; set; }
    public long TotalMilliseconds { get; set; }
    public DateTime LastLapCompletionUtc { get; set; }
    public IEnumerable<Guid> IncludedLaps { get; set; } = Enumerable.Empty<Guid>();
}
