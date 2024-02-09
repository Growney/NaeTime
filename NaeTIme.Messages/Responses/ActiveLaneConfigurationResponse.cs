namespace NaeTime.Messages.Responses;
public record ActiveLaneConfigurationResponse(IEnumerable<ActiveLaneConfigurationResponse.LaneConfiguration> Configurations)
{
    public record LaneConfiguration(byte Lane, Guid? PilotId, byte? BandId, int FrequencyInMhz, bool IsEnabled);
}