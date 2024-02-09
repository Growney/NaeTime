namespace NaeTime.Persistence.Models;
public record ActiveTimings(Guid TrackId, byte Lane, ActiveLap? Lap, ActiveSplit? Split);