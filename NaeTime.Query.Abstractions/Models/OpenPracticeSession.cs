namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSession(Guid Id, string Name, Guid TrackId, IEnumerable<Guid> TrackDetectorIds, IReadOnlyList<OpenPracticeLane> Lanes);