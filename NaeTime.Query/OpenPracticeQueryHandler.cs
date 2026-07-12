using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Query;
public class OpenPracticeQueryHandler : IOpenPracticeQueryHandler
{
    private readonly IOpenPracticeProjection _openPracticeProjection;
    private readonly ITimingProjection _openPracticeTimingProjection;
    private readonly IProjectionProvider _projectionProvider;

    public OpenPracticeQueryHandler(IOpenPracticeProjection openPracticeProjection, ITimingProjection openPracticeTimingProjection, IProjectionProvider projectionProvider)
    {
        _openPracticeProjection = openPracticeProjection;
        _openPracticeTimingProjection = openPracticeTimingProjection;
        _projectionProvider = projectionProvider;
    }

    public Task<OpenPracticeSession?> GetByIdAsync(Guid id) => Task.FromResult(_openPracticeProjection.GetSession(id));

    public Task<SessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId) => 
        _projectionProvider.ClonePullReadPushAsync<SessionTimingInformation,ITimingProjection>(x => x.GetSessionTimingInfo(sessionId, trackId));
    
    public Task<SessionPilotTimingInfo> GetTimingInformation(Guid sessionId, Guid trackId,Guid pilotId) => 
        _projectionProvider.ClonePullReadPushAsync<SessionPilotTimingInfo,ITimingProjection>(x => x.GetSessionPilotTimingInfo(sessionId, trackId, pilotId));
    public Task<Detection?> GetPilotLastDetection(Guid sessionId, Guid trackId, Guid pilotId) => 
        _projectionProvider.ClonePullReadPushAsync<Detection?,ITimingProjection>(x => x.GetLastPilotDetection(sessionId, trackId, pilotId));
    public Task<(TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime)> GetPilotLapTimeOverrides(Guid sessionId, Guid pilotId) => 
        _projectionProvider.ClonePullReadPushAsync<(TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime),IOpenPracticeProjection>(x => x.GetPilotLapTimeOverrides(sessionId, pilotId));
}
