using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IOpenPracticeTimingProjection
{
    OpenPracticeSessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId);
    OpenPracticeSessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId);
    OpenPracticeDetection? GetLastPilotDetection(Guid sessionId, Guid trackId, Guid pilotId);
}