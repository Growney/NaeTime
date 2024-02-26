namespace NaeTime.Messages.Events.OpenPractice;
public record OpenPracticeConsecutiveLapRecordRemoved(Guid SessionId, Guid PilotId, uint LapCap);
