namespace NaeTime.Hardware.Messages;
public record NodeTimerLaneRadioFrequencyConfigured(Guid TimerId, byte Lane, int FrequencyInMhz);
