namespace NaeTime.Persistence.Models;
public record ActiveTimings(Guid TrackId, byte Lane, uint LapNumber, ActiveLap? Lap, ActiveSplit? Split);