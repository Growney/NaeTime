namespace NaeTime.Client.Razor.Lib.Models;
public class LaneConfiguration
{
    public byte LaneNumber { get; set; }
    public int FrequencyInMhz { get; set; }
    public Guid? PilotId { get; set; }
    public bool IsEnabled { get; set; }
}
