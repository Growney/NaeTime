namespace NaeTime.Timing.Models;
public record LaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);