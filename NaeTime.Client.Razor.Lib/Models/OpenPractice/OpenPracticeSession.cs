namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class OpenPracticeSession
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TrackId { get; set; }
    public long MinimumLapMilliseconds { get; set; }
    public long? MaximumLapMilliseconds { get; set; }
    public List<OpenPracticeLap> Laps { get; set; } = new List<OpenPracticeLap>();
    public List<OpenPracticeLaneConfiguration> Lanes { get; set; } = new List<OpenPracticeLaneConfiguration>();
    public List<SingleLapLeaderboard> SingleLapLeaderboards { get; set; } = new List<SingleLapLeaderboard>();
    public List<ConsecutiveLapsLeaderboard> ConsecutiveLapLeaderboards { get; set; } = new List<ConsecutiveLapsLeaderboard>();
}
