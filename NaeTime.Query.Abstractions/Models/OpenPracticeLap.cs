namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeLap(Guid SessionId, Guid TrackId, Guid PilotId, Detection StartDetection, Detection EndDetection, TimeSpan Duration);

