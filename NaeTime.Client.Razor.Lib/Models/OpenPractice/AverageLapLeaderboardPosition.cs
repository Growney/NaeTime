namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class AverageLapLeaderboardPosition
{
    public int Position { get; set; }
    public Guid PilotId { get; set; }
    public double AverageMilliseconds { get; set; }
    public DateTime FirstLapCompletionUtc { get; set; }
}
