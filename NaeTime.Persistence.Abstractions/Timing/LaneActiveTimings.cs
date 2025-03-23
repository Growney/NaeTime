namespace NaeTime.Persistence.Abstractions.Timing;
public record LaneActiveTimings(byte Lane, uint LapNumber, ActiveLap? Lap, ActiveSplit? Split);