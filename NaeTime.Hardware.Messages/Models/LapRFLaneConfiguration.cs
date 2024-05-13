namespace NaeTime.Hardware.Messages.Models;
public record EthernetLapRF8ChannelTimerLaneConfiguration(byte Lane, byte? BandId, int? FrequencyInMhz, bool IsEnabled);