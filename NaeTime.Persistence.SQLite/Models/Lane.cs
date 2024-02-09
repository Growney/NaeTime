namespace NaeTime.Persistence.SQLite.Models;
public class Lane
{
    public byte Id { get; set; }
    public Guid? PilotId { get; set; }
    public byte? BandId { get; set; }
    public int RadioFrequencyInMhz { get; set; }
    public bool IsEnabled { get; set; }
}
