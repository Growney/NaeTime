namespace NaeTime.Persistence.Models;
public record Lane(byte LaneNumber, byte? BandId, int FrequencyInMhz, bool IsEnabled);
