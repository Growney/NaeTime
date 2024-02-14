namespace NaeTime.Persistence.SQLite.Models;
public class ActiveTimings
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public byte Lane { get; set; }
    public uint LapNumber { get; set; }
    public ActiveLap? ActiveLap { get; set; }
    public ActiveSplit? ActiveSplit { get; set; }
}
