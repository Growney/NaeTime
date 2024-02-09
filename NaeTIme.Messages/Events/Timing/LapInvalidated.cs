namespace NaeTime.Messages.Events.Timing;
public record LapInvalidated(Guid SessionId, byte Lane, uint LapNumber, long SoftwareTime, DateTime UtcTime, ulong? HardwareTime, long TotalTime, LapInvalidated.LapInvalidReason Reason)
{
    public enum LapInvalidReason
    {
        TooShort,
        TooLong,
    }
}