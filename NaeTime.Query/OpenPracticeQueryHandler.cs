using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;
public class OpenPracticeQueryHandler(IProjectionProvider projectionProvider) : IOpenPracticeQueryHandler
{
    private readonly IProjectionProvider _projectionProvider = projectionProvider;

    public async Task<OpenPracticeSession?> GetByIdAsync(Guid id)
    {
        OpenPracticeList openPracticeList = await _projectionProvider.Load<OpenPracticeList>();
        return openPracticeList.GetSession(id);
    }

    public async Task<OpenPracticeSessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime)
    {
        OpenPracticeTiming openPracticeTiming = await _projectionProvider.Load<OpenPracticeTiming>();

        return openPracticeTiming.GetSessionTimingInfo(sessionId, trackId, minimumLapTime, maximumLapTime);
    }
}
