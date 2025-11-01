using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IOpenPracticeTimingProjection
{
    OpenPracticeSessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId, TimeSpan minimumLapTime, TimeSpan maximumLapTime);
    OpenPracticeSessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime);
    OpenPracticeDetection? GetLastPilotDetection(Guid sessionId, Guid trackId, Guid pilotId);
}