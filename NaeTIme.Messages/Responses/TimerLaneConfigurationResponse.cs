namespace NaeTime.Messages.Responses;
public record TimerLaneConfigurationResponse(Guid TimerId, IEnumerable<TimerLaneConfigurationResponse.TimerLaneConfiguration> Lanes)
{
    public record TimerLaneConfiguration(byte Lane, byte? bandId, int? FrequencyInMhz, bool IsEnabled);
}
