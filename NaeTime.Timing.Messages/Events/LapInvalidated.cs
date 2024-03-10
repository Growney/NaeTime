namespace NaeTime.Timing.Messages.Events;
public record LapInvalidated(Guid SessionId, byte Lane, uint LapNumber, long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime, long FinishedSoftwareTime, DateTime FinishedUtcTime, ulong? FinishedHardwareTime, long TotalTime, LapInvalidated.LapInvalidReason Reason)
{
    public enum LapInvalidReason
    {
        TooShort,
        TooLong,
        Cancelled,
    }
}