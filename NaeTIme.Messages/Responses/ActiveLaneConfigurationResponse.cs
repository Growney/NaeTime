namespace NaeTime.Messages.Responses;
public record ActiveLaneConfigurationResponse(IEnumerable<ActiveLaneConfigurationResponse.LaneConfiguration> Lanes)
{
    public record LaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);
}