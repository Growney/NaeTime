namespace NaeTime.Query.Abstractions.Models;
public record NaeTimeNodeLane(byte LaneId, bool IsEnabled, ushort EntryThreshold, ushort ExitThreshold, byte? BandId, int FrequencyInMHz) : TuneableLane(BandId, FrequencyInMHz);
