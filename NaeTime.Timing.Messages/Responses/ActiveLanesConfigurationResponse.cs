namespace NaeTime.Timing.Messages.Responses;
public record ActiveLanesConfigurationResponse(IEnumerable<ActiveLanesConfigurationResponse.LaneConfiguration> Lanes)
{
    public record LaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);
}