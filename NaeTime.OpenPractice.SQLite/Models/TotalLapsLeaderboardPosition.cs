namespace NaeTime.OpenPractice.SQLite.Models;
public class TotalLapsLeaderboardPosition
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public Guid SessionId { get; set; }
    public Guid PilotId { get; set; }
    public int TotalLaps { get; set; }
    public DateTime FirstLapCompletionUtc { get; set; }
}
