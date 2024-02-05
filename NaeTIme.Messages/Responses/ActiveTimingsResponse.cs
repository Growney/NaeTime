namespace NaeTime.Messages.Responses;
public record ActiveTimingsResponse(Guid SessionId, byte Lane, ActiveTimingsResponse.ActiveLap? Lap, ActiveTimingsResponse.ActiveSplit? Split)
{
    public record ActiveLap(uint LapNumber, long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);
    public record ActiveSplit(uint LapNumber, byte SplitNumber, long StartedSoftwareTime, DateTime StartedUtcTime);

}