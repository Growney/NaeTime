using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Projections.Abstractions;
public interface IOpenPracticeTimingProjection
{
    OpenPracticeSessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId);
    OpenPracticeSessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime);
    OpenPracticeDetection? GetLastPilotDetection(Guid sessionId, Guid trackId, Guid pilotId);
}