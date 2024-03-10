namespace NaeTime.Hardware.Messages.Messages;
public record TimerLaneRadioFrequencyConfigured(Guid TimerId, byte Lane, int FrequencyInMhz);
