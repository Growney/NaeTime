namespace NaeTime.Timing.Messages.Models;
public record SessionActiveTimings(Guid SessionId, IEnumerable<LaneActiveTimings> Timings);