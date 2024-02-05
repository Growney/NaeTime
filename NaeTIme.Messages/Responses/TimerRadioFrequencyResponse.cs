namespace NaeTime.Messages.Responses;
public record TimerRadioFrequencyResponse(Guid TimerId, IEnumerable<TimerRadioFrequencyResponse.LaneRadioFrequency> LaneFrequencies)
{
    public record LaneRadioFrequency(byte Lane, int FrequencyInMhz, bool IsEnabled);
}
