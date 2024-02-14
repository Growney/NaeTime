namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class OpenPracticeLaneConfiguration
{
    public byte LaneNumber { get; set; }
    public byte? BandId { get; set; }
    public Guid? PilotId { get; set; }
    public int FrequencyInMhz { get; set; }
    public float RssiValue { get; set; }
    public float MaxRssiValue { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LapStarted { get; set; }
}
