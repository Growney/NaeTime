namespace NaeTime.OpenPractice.Messages.Requests;
public record ConsecutiveLapRecordsRequest(Guid SessionId, uint LapCap);
