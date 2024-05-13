namespace NaeTime.Timing.Messages.Models;
public record ActiveLaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);