namespace NaeTime.Management.Messages.Models;
public record ActiveLaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);