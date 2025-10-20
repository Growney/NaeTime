namespace NaeTime.Query.Abstractions.Models;
public record TuneableLane(RequestableValue<byte?> BandId, RequestableValue<int> FrequencyInMHz);
