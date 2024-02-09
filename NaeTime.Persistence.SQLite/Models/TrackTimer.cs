namespace NaeTime.Persistence.SQLite.Models;
public class TrackTimer
{
    public Guid Id { get; set; }
    public Guid TrackId { get; set; }
    public Guid TimerId { get; set; }
}
