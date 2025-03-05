namespace NaeTime.Management.Messages;
public record LaneRadioFrequencyConfigured(byte LaneNumber, byte? BandId, int FrequencyInMhz);

