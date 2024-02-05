namespace NaeTime.Messages.Responses;
public record TimerDetailsResponse(IEnumerable<TimerDetailsResponse.TimerDetails> Timers)
{
    public enum TimerType
    {
        EthernetLapRF8Channel,
    }
    public record TimerDetails(Guid Id, string? Name, TimerType Type);
}
