namespace NaeTime.Timing.SQLite.Models;
public class Lane
{
    public byte Id { get; set; }
    public byte? BandId { get; set; }
    public int FrequencyInMhz { get; set; }
    public bool IsEnabled { get; set; }
}
