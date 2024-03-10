namespace NaeTime.Timing.Messages.Events;
public record LaneRadioFrequencyConfigured(byte LaneNumber, byte? BandId, int FrequencyInMhz);

