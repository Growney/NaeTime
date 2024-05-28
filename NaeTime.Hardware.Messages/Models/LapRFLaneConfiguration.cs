namespace NaeTime.Hardware.Messages.Models;
public record LapRFLaneConfiguration(byte Lane, byte? BandId, int? FrequencyInMhz, bool IsEnabled, ushort Gain, float Threshold);