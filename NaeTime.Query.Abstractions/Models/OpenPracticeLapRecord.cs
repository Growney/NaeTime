namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeLapRecord(Guid SessionId, Guid TrackId, Guid PilotId, uint LapCount, IEnumerable<OpenPracticeLap> IncludedLaps, TimeSpan Record);
