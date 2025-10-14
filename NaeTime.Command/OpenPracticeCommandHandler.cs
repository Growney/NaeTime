using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;

namespace NaeTime.Command;
public class OpenPracticeCommandHandler : IOpenPracticeCommandHandler
{
    private readonly IAggregateRepository _repository;
    private readonly ITrackQueryHandler _trackQueryHandler;

    public OpenPracticeCommandHandler(IAggregateRepository repository, ITrackQueryHandler trackQueryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _trackQueryHandler = trackQueryHandler ?? throw new ArgumentNullException(nameof(trackQueryHandler));
    }

    public async Task AddDetectionToSession(Guid detectionId, Guid sessionId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session does not exist");
        }
        session.AddDetectionToSession(detectionId, lane, hardwareTime, softwareTime, utcTime);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task AssignDetectionToPilot(Guid detectionId, Guid sessionId, Guid pilotId)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session does not exist");
        }
        session.AssignDetectionToPilot(detectionId, pilotId);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task CloneSession(Guid newId, Guid existingId, string newName)
    {
        OpenPracticeSession? existingSession = await _repository.Get<OpenPracticeSession,Guid>(existingId);
        if (existingSession == null)
        {
            throw new ArgumentException($"Session with ID {existingId} does not exist.", nameof(existingId));
        }
        OpenPracticeSession newSession = existingSession.Clone(newId, newName);
        await _repository.Save<OpenPracticeSession, Guid>(newSession);
    }

    public async Task CloneSessionOnNewTrack(Guid newId, Guid existingId, string newName, Guid trackId)
    {
        Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(trackId);
        if (track == null)
        {
            throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        }
        OpenPracticeSession? existingSession = await _repository.Get<OpenPracticeSession, Guid>(existingId);
        if (existingSession == null)
        {
            throw new ArgumentException($"Session with ID {existingId} does not exist.", nameof(existingId));
        }
        OpenPracticeSession newSession = existingSession.Clone(newId, trackId, newName);
        newSession.ConfigureDefaultLanes(track.MaxLanes);
        await _repository.Save<OpenPracticeSession, Guid>(newSession);
    }

    public async Task DisableLane(Guid sessionId, byte lane)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        }
        session.DisableLane(lane);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task EnableLane(Guid sessionId, byte lane)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        }
        session.EnableLane(lane);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task RenameSession(Guid sessionId, string name)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        }
        session.Rename(name);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task ResetLanePilot(Guid sessionId, byte lane)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        }

        session.ResetLanePilot(lane);

        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task ScheduleSession(Guid id, Guid trackId, string name)
    {
        Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(trackId);

        if (track == null)
        {
            throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        }

        OpenPracticeSession session = _repository.CreateNew<OpenPracticeSession,Guid>(() => new OpenPracticeSession(id, trackId, name));
        session.ConfigureDefaultLanes(track.MaxLanes);

        await _repository.Save<OpenPracticeSession,Guid>(session);
    }

    public async Task SetLanePilot(Guid sessionId, byte lane, Guid pilotId)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        }

        session.SetLanePilot(lane, pilotId);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task TuneLane(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        }
        session.TuneLaneVideoFrequency(lane, bandId, frequencyInMhz);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task RemoveDetectionFromSession(Guid detectionId, Guid sessionId)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session does not exist");
        }
        session.RemoveDetectionFromSession(detectionId);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }

    public async Task UnassignDetectionFromPilot(Guid detectionId,Guid sessionId, Guid pilotId)
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session does not exist");
        }
        session.UnassignDetectionFromPilot(detectionId);
        await _repository.Save<OpenPracticeSession, Guid>(session);
    }
}
