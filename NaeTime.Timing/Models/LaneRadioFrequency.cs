namespace NaeTime.Timing.Models;
public record LaneRadioFrequency(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);