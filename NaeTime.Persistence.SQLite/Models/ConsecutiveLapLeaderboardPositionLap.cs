namespace NaeTime.Persistence.SQLite.Models;
public class ConsecutiveLapLeaderboardPositionLap
{
    public Guid Id { get; set; }
    public int Ordinal { get; set; }
    public Guid LapId { get; set; }
}
