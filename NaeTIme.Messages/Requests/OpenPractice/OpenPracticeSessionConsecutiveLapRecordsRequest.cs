namespace NaeTime.Messages.Requests.OpenPractice;
public record OpenPracticeSessionConsecutiveLapRecordsRequest(Guid SessionId, uint LapCap);
