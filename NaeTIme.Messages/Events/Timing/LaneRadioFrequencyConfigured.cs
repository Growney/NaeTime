namespace NaeTime.Messages.Events.Timing;
public record LaneRadioFrequencyConfigured(byte LaneNumber, byte? BandId, int FrequencyInMhz);

