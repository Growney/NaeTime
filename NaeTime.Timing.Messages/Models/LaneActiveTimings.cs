namespace NaeTime.Timing.Messages.Models;
public record LaneActiveTimings(byte Lane, uint LapNumber, ActiveLap? Lap, ActiveSplit? Split);