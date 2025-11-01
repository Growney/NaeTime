using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Query;
public class OpenPracticeQueryHandler : IOpenPracticeQueryHandler
{
    private readonly IOpenPracticeProjection _openPracticeProjection;
    private readonly IOpenPracticeTimingProjection _openPracticeTimingProjection;

    public OpenPracticeQueryHandler(IOpenPracticeProjection openPracticeProjection, IOpenPracticeTimingProjection openPracticeTimingProjection)
    {
        _openPracticeProjection = openPracticeProjection;
        _openPracticeTimingProjection = openPracticeTimingProjection;
    }

    public Task<OpenPracticeSession?> GetByIdAsync(Guid id) => Task.FromResult(_openPracticeProjection.GetSession(id));

    public Task<OpenPracticeSessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime) => Task.FromResult(_openPracticeTimingProjection.GetSessionTimingInfo(sessionId, trackId, minimumLapTime, maximumLapTime));
    public Task<OpenPracticeSessionPilotTimingInfo> GetTimingInformation(Guid sessionId, Guid trackId,Guid pilotId, TimeSpan minimumLapTime, TimeSpan maximumLapTime) => Task.FromResult(_openPracticeTimingProjection.GetSessionPilotTimingInfo(sessionId, trackId, pilotId, minimumLapTime, maximumLapTime));
    public Task<OpenPracticeDetection?> GetPilotLastDetection(Guid sessionId, Guid trackId, Guid pilotId) => Task.FromResult(_openPracticeTimingProjection.GetLastPilotDetection(sessionId, trackId, pilotId));
}
