namespace NaeTime.OpenPractice.Messages.Events;
public record ConsecutiveLapRecordRemoved(Guid SessionId, Guid PilotId, uint LapCap);
