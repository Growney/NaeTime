namespace NaeTime.Timing.Messages.Models;
public record LaneConfiguration(byte Lane, Guid? PilotId, int FrequencyInMhz);
