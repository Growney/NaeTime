using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Query;
public class OpenPracticeQueryHandler : IOpenPracticeQueryHandler
{
    private readonly IOpenPracticeProjection _openPracticeProjection;
    private readonly IOpenPracticeTimingProjection _openPracticeTimingProjection;
    private readonly IProjectionProvider _projectionProvider;

    public OpenPracticeQueryHandler(IOpenPracticeProjection openPracticeProjection, IOpenPracticeTimingProjection openPracticeTimingProjection, IProjectionProvider projectionProvider)
    {
        _openPracticeProjection = openPracticeProjection;
        _openPracticeTimingProjection = openPracticeTimingProjection;
        _projectionProvider = projectionProvider;
    }

    public Task<OpenPracticeSession?> GetByIdAsync(Guid id) => Task.FromResult(_openPracticeProjection.GetSession(id));

    public Task<OpenPracticeSessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId) => 
        _projectionProvider.ClonePullReadPushAsync<OpenPracticeSessionTimingInformation,IOpenPracticeTimingProjection>(x => x.GetSessionTimingInfo(sessionId, trackId));
    
    public Task<OpenPracticeSessionPilotTimingInfo> GetTimingInformation(Guid sessionId, Guid trackId,Guid pilotId) => 
        _projectionProvider.ClonePullReadPushAsync<OpenPracticeSessionPilotTimingInfo,IOpenPracticeTimingProjection>(x => x.GetSessionPilotTimingInfo(sessionId, trackId, pilotId));
    public Task<OpenPracticeDetection?> GetPilotLastDetection(Guid sessionId, Guid trackId, Guid pilotId) => 
        _projectionProvider.ClonePullReadPushAsync<OpenPracticeDetection?,IOpenPracticeTimingProjection>(x => x.GetLastPilotDetection(sessionId, trackId, pilotId));
    public Task<(TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime)> GetPilotLapTimeOverrides(Guid sessionId, Guid pilotId) => 
        _projectionProvider.ClonePullReadPushAsync<(TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime),IOpenPracticeProjection>(x => x.GetPilotLapTimeOverrides(sessionId, pilotId));
}
