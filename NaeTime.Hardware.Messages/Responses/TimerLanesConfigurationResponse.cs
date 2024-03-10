namespace NaeTime.Hardware.Messages.Responses;
public record TimerLanesConfigurationResponse(Guid TimerId, IEnumerable<TimerLanesConfigurationResponse.TimerLaneConfiguration> Lanes)
{
    public record TimerLaneConfiguration(byte Lane, byte? bandId, int? FrequencyInMhz, bool IsEnabled);
}
