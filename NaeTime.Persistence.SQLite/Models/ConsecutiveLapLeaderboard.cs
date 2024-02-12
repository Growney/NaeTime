namespace NaeTime.Persistence.SQLite.Models;
public record ConsecutiveLapLeaderboard
{
    public Guid Id { get; set; }
    public uint ConsecutiveLaps { get; set; }
    public List<ConsecutiveLapLeaderboardPosition> Positions { get; set; } = new List<ConsecutiveLapLeaderboardPosition>();
}

