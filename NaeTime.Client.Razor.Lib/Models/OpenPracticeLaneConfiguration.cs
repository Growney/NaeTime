namespace NaeTime.Client.Razor.Lib.Models;
public class OpenPracticeLaneConfiguration
{
    public byte Lane { get; set; }
    public Guid? PilotId { get; set; }
    public bool IsEnabled { get; set; }
    public byte? BandId { get; set; }
    public int? FrequencyInMhz { get; set; }
}
