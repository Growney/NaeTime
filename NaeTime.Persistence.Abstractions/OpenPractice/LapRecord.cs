namespace NaeTime.Persistence.Abstractions.OpenPractice;
public record LapRecord(uint LapCap, IEnumerable<Guid> LapIds);