namespace NaeTime.Timing.Messages.Responses;
public record ActiveTimingsResponse(Guid SessionId, IEnumerable<ActiveTimingsResponse.ActiveTimings> Timings)
{
    public record ActiveTimings(byte Lane, uint lapNumber, ActiveLap? Lap, ActiveSplit? Split);
    public record ActiveSplit(byte SplitNumber, long StartedSoftwareTime, DateTime StartedUtcTime);
    public record ActiveLap(long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);
}
