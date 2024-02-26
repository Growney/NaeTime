namespace NaeTime.Messages.Events.OpenPractice;
public record OpenPracticeConsecutiveLapCountTracked(Guid SessionId, uint LapCap);