using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using EventDbLite.Streams;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;

namespace NaeTime.Command;
public class OpenPracticePilotTimingCommandHandler : IOpenPracticePilotTimingCommandHandler
{
    private readonly IAggregateRepository _repository;
    private readonly IOpenPracticeQueryHandler _openPracticeQueryHandler;
    private readonly ITrackQueryHandler _trackQueryHandler;

    public OpenPracticePilotTimingCommandHandler(IAggregateRepository repository, IOpenPracticeQueryHandler openPracticeQueryHandler, ITrackQueryHandler trackQueryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _openPracticeQueryHandler = openPracticeQueryHandler ?? throw new ArgumentNullException(nameof(openPracticeQueryHandler));
        _trackQueryHandler = trackQueryHandler ?? throw new ArgumentNullException(nameof(trackQueryHandler));
    }

    public async Task AddDetectionOccurance(Guid DetectionId, Guid SessionId, Guid PilotId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime)
    {
        Query.Abstractions.Models.OpenPracticeSession? session = await _openPracticeQueryHandler.GetByIdAsync(SessionId)
            ?? throw new ArgumentNullException("Session not found");

        OpenPracticePilotTiming timing = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticePilotTimingId>(new OpenPracticePilotTiming.OpenPracticePilotTimingId { PilotId = PilotId, SessionId = SessionId, TrackId = session.TrackId })
            ?? throw new ArgumentNullException("Pilot timing not started");

        timing.AddDetectionOccurance(DetectionId, TimerId, Lane, HardwareTime, SoftwareTime, UtcTime);

        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticePilotTimingId>(timing);
    }

    public async Task StartPilotTimingSession(Guid SessionId, Guid PilotId)
    {
        Query.Abstractions.Models.OpenPracticeSession? session = await _openPracticeQueryHandler.GetByIdAsync(SessionId)
            ?? throw new ArgumentNullException("Session not found");

        Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(session.TrackId)
            ?? throw new ArgumentNullException("Track not found");

        OpenPracticePilotTiming timing = _repository.CreateNew(() => new OpenPracticePilotTiming(PilotId, SessionId, session.TrackId, track.Detectors.Select(x => x.Id).ToArray(), track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds));

        try
        {
            await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticePilotTimingId>(timing, StreamPosition.NoStream);
        }
        catch (ConcurrencyException)
        {

        }
    }

    public async Task TriggerDetection(Guid DetectionId, Guid SessionId, Guid PilotId, byte lane, byte OrdinalPosition, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime)
    {
        Query.Abstractions.Models.OpenPracticeSession? session = await _openPracticeQueryHandler.GetByIdAsync(SessionId)
            ?? throw new ArgumentNullException("Session not found");

        OpenPracticePilotTiming timing = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticePilotTimingId>(new OpenPracticePilotTiming.OpenPracticePilotTimingId { PilotId = PilotId, SessionId = SessionId, TrackId = session.TrackId })
            ?? throw new ArgumentNullException("Pilot timing not started");

        timing.TriggerDetection(DetectionId, lane, OrdinalPosition, HardwareTime, SoftwareTime, UtcTime);

        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticePilotTimingId>(timing);
    }
}
