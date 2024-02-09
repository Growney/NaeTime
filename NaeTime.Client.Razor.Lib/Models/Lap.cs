namespace NaeTime.Client.Razor.Lib.Models;
public class Lap
{
    public Guid TrackId { get; set; }
    public byte Lane { get; set; }
    public uint LapNumber { get; set; }
    public LapStatus Status { get; set; }
    public DateTime Started { get; set; }
    public DateTime? Ended { get; set; }
    public long? TotalTime { get; set; }
}
