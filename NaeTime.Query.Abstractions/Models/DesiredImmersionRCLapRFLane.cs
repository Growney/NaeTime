namespace NaeTime.Query.Abstractions.Models;
public record DesiredImmersionRCLapRFLane(byte LaneId, bool? IsEnabled, ushort? Gain, float? Threshold, byte? BandId, int? FrequencyInMHz);
