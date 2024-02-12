namespace NaeTime.Persistence.SQLite.Models;
public class SingleLapLeaderboard
{
    public Guid Id { get; set; }

    public List<SingleLapLeaderboardPosition> Positions { get; set; } = new List<SingleLapLeaderboardPosition>();
}

