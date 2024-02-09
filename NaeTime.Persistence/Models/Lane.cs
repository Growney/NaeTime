namespace NaeTime.Persistence.Models;
public record Lane(byte LaneNumber, Guid? Pilot, byte? BandId, int FrequencyInMhz, bool IsEnabled);
