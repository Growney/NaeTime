namespace NaeTime.Persistence.SQLite.Models;
public class Track
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public long MinimumLapMilliseconds { get; set; }
    public long MaximumLapMilliseconds { get; set; }
    public List<TrackTimer> Timers { get; set; } = new();
}
