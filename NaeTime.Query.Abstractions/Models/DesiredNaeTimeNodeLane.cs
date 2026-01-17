namespace NaeTime.Query.Abstractions.Models;
public record DesiredNaeTimeNodeLane(byte LaneId, bool? IsEnabled, ushort? EntryThreshold, ushort? ExitThreshold, byte? BandId, int? FrequencyInMHz);