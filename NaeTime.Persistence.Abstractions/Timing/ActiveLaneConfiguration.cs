namespace NaeTime.Persistence.Abstractions.Timing;
public record ActiveLaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);