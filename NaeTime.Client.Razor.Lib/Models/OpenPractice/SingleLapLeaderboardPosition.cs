namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class SingleLapLeaderboardPosition
{
    public int Position { get; set; }
    public Guid PilotId { get; set; }
    public long TotalMilliseconds { get; set; }
    public DateTime CompletionUtc { get; set; }
    public Guid LapId { get; set; }
}
