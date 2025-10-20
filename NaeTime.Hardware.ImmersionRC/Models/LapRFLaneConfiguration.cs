namespace NaeTime.Hardware.ImmersionRC.Models;
public record LapRFLaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled, ushort Gain, float Threshold);
