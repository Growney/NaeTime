using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IOpenPracticeProjection
{
    OpenPracticeSession? GetSession(Guid sessionId);
    (TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime) GetPilotLapTimeOverrides(Guid sessionId, Guid pilotId);
}
