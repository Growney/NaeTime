namespace NaeTime.Persistence.Models;
public record Lane(byte LaneNumber, Guid? Pilot, int FrequencyInMhz, bool IsEnabled);
