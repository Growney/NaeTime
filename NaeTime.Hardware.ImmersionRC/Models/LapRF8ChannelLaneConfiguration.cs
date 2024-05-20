namespace NaeTime.Hardware.ImmersionRC.Models;
public record LapRF8ChannelLaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled, ushort Gain, float Threshold);
