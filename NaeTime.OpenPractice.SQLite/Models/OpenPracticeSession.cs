namespace NaeTime.OpenPractice.SQLite.Models;
public class OpenPracticeSession
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TrackId { get; set; }
    public long MinimumLapMilliseconds { get; set; }
    public long? MaximumLapMilliseconds { get; set; }
    public List<PilotLane> ActiveLanes { get; set; } = new List<PilotLane>();
    public List<TrackedConsecutiveLaps> TrackedConsecutiveLaps { get; set; } = new List<TrackedConsecutiveLaps>();
}
