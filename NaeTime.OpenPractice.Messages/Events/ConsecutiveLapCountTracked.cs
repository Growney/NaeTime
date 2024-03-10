namespace NaeTime.OpenPractice.Messages.Events;
public record ConsecutiveLapCountTracked(Guid SessionId, uint LapCap);