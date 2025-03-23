namespace NaeTime.Persistence.Abstractions.Timing;
public record SessionActiveTimings(Guid SessionId, IEnumerable<LaneActiveTimings> Timings);