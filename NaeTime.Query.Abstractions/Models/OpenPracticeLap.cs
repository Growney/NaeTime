namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeLap(Guid SessionId, Guid TrackId, Guid PilotId, OpenPracticeDetection StartDetection, OpenPracticeDetection EndDetection, TimeSpan Duration);

