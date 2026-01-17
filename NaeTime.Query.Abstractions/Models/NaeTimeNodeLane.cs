namespace NaeTime.Query.Abstractions.Models;
public record NaeTimeNodeLane(byte LaneId, RequestableValue<bool?> IsEnabled, RequestableValue<ushort?> EntryThreshold, RequestableValue<ushort?> ExitThreshold, RequestableValue<byte?> BandId, RequestableValue<int?> FrequencyInMHz) : TuneableLane(BandId, FrequencyInMHz);
