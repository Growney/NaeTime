namespace NaeTime.Messages.Responses;
public record TimerLaneConfigurationResponse(Guid TimerId, IEnumerable<TimerLaneConfigurationResponse.TimerLaneConfiguration> Lanes)
{
    public record TimerLaneConfiguration(byte Lane, int? FrequencyInMhz, Guid? PilotId, bool IsEnabled);
}
