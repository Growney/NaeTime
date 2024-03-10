namespace NaeTime.OpenPractice.Messages.Responses;
public record TrackedConsecutiveLapsResponse(Guid SessionId, IEnumerable<uint> ConsecutiveLaps);
