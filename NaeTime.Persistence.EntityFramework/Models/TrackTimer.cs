namespace NaeTime.Persistence.EntityFramework.Models;
public class TrackTimer
{
    public Guid Id { get; set; }
    public Guid TrackId { get; set; }
    public Guid TimerId { get; set; }
    public int OrdinalPosition { get; set; }
}
