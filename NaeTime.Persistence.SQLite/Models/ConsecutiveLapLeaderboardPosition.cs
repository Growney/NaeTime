namespace NaeTime.Persistence.SQLite.Models;
public class ConsecutiveLapLeaderboardPosition
{
    public Guid Id { get; set; }
    public uint Position { get; set; }
    public Guid PilotId { get; set; }
    public uint StartLapNumber { get; set; }
    public uint EndLapNumber { get; set; }
    public uint TotalLaps { get; set; }
    public long TotalMilliseconds { get; set; }
    public DateTime LastLapCompletionUtc { get; set; }
}

