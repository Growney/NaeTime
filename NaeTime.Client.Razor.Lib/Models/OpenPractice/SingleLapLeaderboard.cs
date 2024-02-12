namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class SingleLapLeaderboard
{
    public Guid Id { get; set; }

    public List<SingleLapLeaderboardPosition> Positions { get; set; } = new List<SingleLapLeaderboardPosition>();
}

