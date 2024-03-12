namespace NaeTime.OpenPractice.SQLite.Models;
public class SingleLapLeaderboardPosition
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public Guid SessionId { get; set; }
    public Guid PilotId { get; set; }
    public long TotalMilliseconds { get; set; }
    public DateTime CompletionUtc { get; set; }
    public Guid LapId { get; set; }
}
