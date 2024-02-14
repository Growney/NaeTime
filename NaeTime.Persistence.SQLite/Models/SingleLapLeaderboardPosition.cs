namespace NaeTime.Persistence.SQLite.Models;
public class SingleLapLeaderboardPosition
{
    public Guid Id { get; set; }
    public uint Position { get; set; }
    public Guid PilotId { get; set; }
    public Guid LapId { get; set; }
    public long LapMilliseconds { get; set; }
    public DateTime CompletionUtc { get; set; }
}

