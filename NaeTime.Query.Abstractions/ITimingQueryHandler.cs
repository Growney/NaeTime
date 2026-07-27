using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions;
public interface ITimingQueryHandler
{
    Task<OpenPracticeSession?> GetByIdAsync(Guid id);
    Task<Guid?> GetLanePilot(Guid sessionId,byte lane);
    Task<SessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId);
    Task<SessionPilotTimingInfo> GetTimingInformation(Guid sessionId, Guid trackId,Guid pilotId);
    Task<Detection?> GetPilotLastDetection(Guid sessionId, Guid trackId, Guid pilotId);
    Task<(TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime)> GetPilotLapTimeOverrides(Guid sessionId, Guid pilotId);
}
