namespace NaeTime.Timing.Messages.Responses;
public record ActiveTimingResponse(Guid SessionId, byte Lane, uint LapNumber, ActiveTimingResponse.ActiveLap? Lap, ActiveTimingResponse.ActiveSplit? Split)
{
    public record ActiveLap(long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);
    public record ActiveSplit(byte SplitNumber, long StartedSoftwareTime, DateTime StartedUtcTime);

}