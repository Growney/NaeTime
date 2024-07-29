namespace NaeTime.OpenPractice.Messages.Models;
public record LapRecord(uint LapCap, IEnumerable<Guid> LapIds);