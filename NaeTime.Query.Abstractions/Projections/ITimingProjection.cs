using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface ITimingProjection
{
    SessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId);
    SessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId);
    Detection? GetLastPilotDetection(Guid sessionId, Guid trackId, Guid pilotId);
    Detection? GetDetection(Guid detectionId);
}