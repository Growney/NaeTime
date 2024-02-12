namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public record ConsecutiveLapsLeaderboard
{
    public Guid Id { get; set; }
    public uint ConsecutiveLaps { get; set; }
    public List<ConsecutiveLapsLeaderboardPosition> Positions { get; set; } = new List<ConsecutiveLapsLeaderboardPosition>();
}

