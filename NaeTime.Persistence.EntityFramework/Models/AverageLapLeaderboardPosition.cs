namespace NaeTime.Persistence.EntityFramework.Models;
public class AverageLapLeaderboardPosition
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public Guid SessionId { get; set; }
    public Guid PilotId { get; set; }
    public double AverageMilliseconds { get; set; }
    public DateTime FirstLapCompletionUtc { get; set; }
}
