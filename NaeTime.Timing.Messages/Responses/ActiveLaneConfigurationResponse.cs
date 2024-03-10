namespace NaeTime.Timing.Messages.Responses;
public record ActiveLaneConfigurationResponse(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled);