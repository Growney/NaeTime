namespace NaeTime.Persistence.Abstractions.Hardware;
public record LaneConfiguration(byte Lane, bool? IsEnabled, byte? BandId, int? FrequencyInMhz, IEnumerable<TimerLaneConfiguredField> Fields);