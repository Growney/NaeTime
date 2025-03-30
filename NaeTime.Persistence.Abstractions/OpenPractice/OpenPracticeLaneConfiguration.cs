namespace NaeTime.Persistence.Abstractions.OpenPractice;
public record OpenPracticeLaneConfiguration(byte Lane, Guid? PilotId, bool IsEnabled, byte? BandId, int? FrequencyInMhz);