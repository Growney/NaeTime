namespace NaeTime.Timing.Abstractions;
public interface ISession
{
    Task HandleLapStarted(Guid trackId, byte lane, uint lapNumber, long softwareTime, DateTime utcTime, ulong? hardwareTime);
    Task HandleLapCompleted(Guid trackId, byte lane, uint lapNumber, long softwareTime, DateTime utcTime, ulong? hardwareTime, long totalTime);

    Task HandleSplitStarted(Guid trackId, byte lane, uint lapNumber, byte split, long softwareTime, DateTime utcTime);
    Task HandleSplitCompleted(Guid trackId, byte lane, uint lapNumber, byte split, long softwareTime, DateTime utcTime, long totalTime);
    Task HandleSplitSkipped(Guid trackId, byte lane, uint lapNumber, byte split);
}
