namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class TotalLapLeaderboardPosition
{
    public int Position { get; set; }
    public Guid PilotId { get; set; }
    public int TotalLaps { get; set; }
    public DateTime FirstLapCompletionUtc { get; set; }
}
