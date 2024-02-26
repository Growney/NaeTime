namespace NaeTime.Messages.Responses;
public record OpenPracticeSessionTrackedConsecutiveLapsResponse(Guid SessionId, IEnumerable<uint> ConsecutiveLaps);
