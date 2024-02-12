namespace NaeTime.Persistence.SQLite.Models;
public class PilotLane
{
    public Guid Id { get; set; }
    public Guid PilotId { get; set; }
    public byte Lane { get; set; }
}
