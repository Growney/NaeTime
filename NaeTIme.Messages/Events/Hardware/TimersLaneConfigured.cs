namespace NaeTime.Messages.Events.Hardware;
public record TimersLaneConfigured(Guid TimerId, IEnumerable<TimersLaneConfigured.LaneConfiguration> Lanes)
{
    public record LaneConfiguration(byte Lane, int? FrequencyInMhz, bool IsEnabled);
}