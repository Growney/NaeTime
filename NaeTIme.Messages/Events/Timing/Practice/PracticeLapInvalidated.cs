namespace NaeTime.Messages.Events.Timing.Practice;
public record PracticeLapInvalidated(Guid TrackId, byte Lane, Guid? PilotId, uint LapNumber, long SoftwareTime, DateTime UtcTime, ulong? HardwareTime, long TotalTime, PracticeLapInvalidated.LapInvalidReason Reason)
{
    public enum LapInvalidReason
    {
        TooShort,
        TooLong,
    }

}
