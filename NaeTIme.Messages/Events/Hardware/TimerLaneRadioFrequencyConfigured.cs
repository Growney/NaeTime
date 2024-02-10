namespace NaeTime.Messages.Events.Hardware;
public record TimerLaneRadioFrequencyConfigured(Guid TimerId, byte Lane, int FrequencyInMhz);
