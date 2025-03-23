namespace NaeTime.Persistence.Abstractions.Hardware;
public record LapRFLaneConfiguration(byte Lane, byte? BandId, int? FrequencyInMhz, bool IsEnabled, ushort Gain, float Threshold);