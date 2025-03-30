namespace NaeTime.Persistence.EntityFramework.Models;

public class TimerLaneConfiguration
{
    public Guid Id { get; set; }
    public byte LaneId { get; set; }
    public bool DesiredIsEnabled { get; set; }
    public byte? DesiredBandId { get; set; }
    public int? DesiredFrequencyInMhz { get; set; }
    public bool? ActualIsEnabled { get; set; }
    public byte? ActualBandId { get; set; }
    public int? ActualFrequencyInMhz { get; set; }
}
