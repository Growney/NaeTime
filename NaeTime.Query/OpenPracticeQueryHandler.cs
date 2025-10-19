using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;
using NaeTime.Query.Projections.Abstractions;

namespace NaeTime.Query;
public class OpenPracticeQueryHandler : IOpenPracticeQueryHandler
{
    private readonly IProjectionProvider _projectionProvider;
    private readonly IOpenPracticeTimingProjection _openPracticeTimingProjection;

    public OpenPracticeQueryHandler(IProjectionProvider projectionProvider, IOpenPracticeTimingProjection openPracticeTimingProjection)
    {
        _projectionProvider = projectionProvider;
        _openPracticeTimingProjection = openPracticeTimingProjection;
    }

    public async Task<OpenPracticeSession?> GetByIdAsync(Guid id)
    {
        OpenPracticeList openPracticeList = await _projectionProvider.Load<OpenPracticeList>();
        return openPracticeList.GetSession(id);
    }

    public async Task<OpenPracticeSessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime)
    {
        return _openPracticeTimingProjection.GetSessionTimingInfo(sessionId, trackId, minimumLapTime, maximumLapTime);
    }
}
