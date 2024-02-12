namespace NaeTime.Persistence.SQLite.Models;
public class OpenPracticeSession
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TrackId { get; set; }
    public long MinimumLapMilliseconds { get; set; }
    public long? MaximumLapMilliseconds { get; set; }
    public List<OpenPracticeLap> Laps { get; set; } = new List<OpenPracticeLap>();
    public List<PilotLane> ActiveLanes { get; set; } = new List<PilotLane>();
    public List<SingleLapLeaderboard> SingleLapLeaderboards { get; set; } = new List<SingleLapLeaderboard>();
    public List<ConsecutiveLapLeaderboard> ConsecutiveLapLeaderboards { get; set; } = new List<ConsecutiveLapLeaderboard>();
}
