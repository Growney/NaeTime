using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;

namespace NaeTime.Command;
public class OpenPracticeCommandHandler(IAggregateRepository repository, ITrackQueryHandler trackQueryHandler) : IOpenPracticeCommandHandler
{
    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ITrackQueryHandler _trackQueryHandler = trackQueryHandler ?? throw new ArgumentNullException(nameof(trackQueryHandler));

    public async Task CloneSession(Guid newId, Guid existingId, string newName)
    {
        OpenPracticeSession? existingSession = await _repository.Get<OpenPracticeSession, Guid>(existingId) ?? throw new ArgumentException($"Session with ID {existingId} does not exist.", nameof(existingId));
        OpenPracticeSession newSession = existingSession.Clone(newId, newName);
        await _repository.Save<OpenPracticeSession, Guid>(newSession);
    }

    public async Task CloneSessionOnNewTrack(Guid newId, Guid existingId, string newName, Guid trackId)
    {
        Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        OpenPracticeSession? existingSession = await _repository.Get<OpenPracticeSession, Guid>(existingId) ?? throw new ArgumentException($"Session with ID {existingId} does not exist.", nameof(existingId));
        OpenPracticeSession newSession = existingSession.Clone(newId, trackId, newName);
        newSession.ConfigureDefaultLanes(track.MaxLanes);
        await _repository.Save<OpenPracticeSession, Guid>(newSession);
    }

    public async Task DisableLane(Guid sessionId, byte lane)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.DisableLane(lane);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task EnableLane(Guid sessionId, byte lane)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.EnableLane(lane);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task RenameSession(Guid sessionId, string name)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.Rename(name);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task ResetLanePilot(Guid sessionId, byte lane)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.ResetLanePilot(lane);

        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task ScheduleSession(Guid id, Guid trackId, string name)
    {
        Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        OpenPracticeSession session = _repository.CreateNew<OpenPracticeSession>(() => new OpenPracticeSession(id, trackId, name));
        session.ConfigureDefaultLanes(track.MaxLanes);

        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task SetLanePilot(Guid sessionId, byte lane, Guid pilotId)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.SetLanePilot(lane, pilotId);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task TuneLane(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.TuneLaneVideoFrequency(lane, bandId, frequencyInMhz);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

}
