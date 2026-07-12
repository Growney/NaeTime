using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Hardware.Abstractions;
using NaeTime.Query.Abstractions;
using System.Numerics;
using System.Xml.Linq;

namespace NaeTime.Command;

public class OpenPracticeCommandHandler : IOpenPracticeCommandHandler
{
    private readonly IAggregateRepository _repository;
    private readonly ITrackQueryHandler _trackQueryHandler;
    private readonly ISoftwareTimer _softwareTimer;
    public OpenPracticeCommandHandler(IAggregateRepository repository, ITrackQueryHandler trackQueryHandler, ISoftwareTimer softwareTimer)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _trackQueryHandler = trackQueryHandler ?? throw new ArgumentNullException(nameof(trackQueryHandler));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public Task CloneSession(Guid newId, Guid existingId, string newName) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSession? existingSession = await _repository.Get<OpenPracticeSession, Guid>(existingId).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {existingId} does not exist.", nameof(existingId));
        existingSession.Clone(newId, newName, null);
        await _repository.Save<OpenPracticeSession, Guid>(existingSession).ConfigureAwait(false);
    });

    public Task CloneSessionOnNewTrack(Guid newId, Guid existingId, string newName, Guid trackId) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSession? existingSession = await _repository.Get<OpenPracticeSession, Guid>(existingId).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {existingId} does not exist.", nameof(existingId));
        existingSession.Clone(newId, newName, trackId);
        await _repository.Save<OpenPracticeSession, Guid>(existingSession).ConfigureAwait(false);
    });

    public Task DisableLane(Guid sessionId, byte lane) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSessionLane? session = await _repository.Get<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(new OpenPracticeSessionLane.LaneKey(sessionId, lane)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.DisableLane();
        await _repository.Save<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(session).ConfigureAwait(false);
    });

    public Task EnableLane(Guid sessionId, byte lane) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSessionLane? session = await _repository.Get<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(new OpenPracticeSessionLane.LaneKey(sessionId, lane)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.EnableLane();
        await _repository.Save<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(session).ConfigureAwait(false);
    });
    public Task RenameSession(Guid sessionId, string name) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.Rename(name);
        await _repository.Save<OpenPracticeSession, Guid>(session).ConfigureAwait(false);
    });

    public Task ResetLanePilot(Guid sessionId, byte lane) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSessionLane? session = await _repository.Get<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(new OpenPracticeSessionLane.LaneKey(sessionId, lane)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.ResetLanePilot();
        await _repository.Save<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(session).ConfigureAwait(false);
    });

    public Task ScheduleSession(Guid id, Guid trackId, string name, TimeSpan? minimumLapTime, TimeSpan? maximumLapTime) => ConcurrencyException.Retry(async () =>
    {
        Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(trackId).ConfigureAwait(false) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        OpenPracticeSession session = _repository.CreateNew<OpenPracticeSession>(() => new OpenPracticeSession(id, trackId, name, minimumLapTime, maximumLapTime));

        await _repository.Save<OpenPracticeSession, Guid>(session).ConfigureAwait(false);
    });

    public Task SetLanePilot(Guid sessionId, byte lane, Guid pilotId) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSessionLane? session = await _repository.Get<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(new OpenPracticeSessionLane.LaneKey(sessionId, lane)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.SetLanePilot(pilotId);
        await _repository.Save<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(session).ConfigureAwait(false);
    });

    public Task TuneLane(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSessionLane? session = await _repository.Get<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(new OpenPracticeSessionLane.LaneKey(sessionId, lane)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.TuneLaneVideoFrequency(bandId, frequencyInMhz);
        await _repository.Save<OpenPracticeSessionLane, OpenPracticeSessionLane.LaneKey>(session).ConfigureAwait(false);
    });

    public Task SetMinimumLapTime(Guid sessionId, TimeSpan minimumLapTime) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.SetMinimumLapTime(minimumLapTime);
        await _repository.Save<OpenPracticeSession, Guid>(session).ConfigureAwait(false);
    });

    public Task ResetMinimumLapTime(Guid sessionId) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.ResetMinimumLapTime();
        await _repository.Save<OpenPracticeSession, Guid>(session).ConfigureAwait(false);
    });

    public Task SetMaximumLapTime(Guid sessionId, TimeSpan maximumLapTime) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.SetMaximumLapTime(maximumLapTime);
        await _repository.Save<OpenPracticeSession, Guid>(session).ConfigureAwait(false);
    });

    public Task ResetMaximumLapTime(Guid sessionId) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticeSession? session = await _repository.Get<OpenPracticeSession, Guid>(sessionId).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.ResetMaximumLapTime();
        await _repository.Save<OpenPracticeSession, Guid>(session).ConfigureAwait(false);
    });

    public Task SetPilotMinimumLapTime(Guid sessionId, Guid pilotId, TimeSpan minimumLapTime) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticePilotTiming? session = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(new OpenPracticePilotTiming.PilotKey(sessionId, pilotId)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.SetMinimumLapTime(minimumLapTime);
        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(session).ConfigureAwait(false);
    });

    public Task ResetPilotMinimumLapTime(Guid sessionId, Guid pilotId) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticePilotTiming? session = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(new OpenPracticePilotTiming.PilotKey(sessionId, pilotId)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.ResetMinimumLapTime();
        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(session).ConfigureAwait(false);
    });

    public Task SetPilotMaximumLapTime(Guid sessionId, Guid pilotId, TimeSpan maximumLapTime) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticePilotTiming? session = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(new OpenPracticePilotTiming.PilotKey(sessionId, pilotId)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.SetMaximumLapTime(maximumLapTime);
        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(session).ConfigureAwait(false);
    });

    public Task ResetPilotMaximumLapTime(Guid sessionId, Guid pilotId) => ConcurrencyException.Retry(async () =>
    {
        OpenPracticePilotTiming? session = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(new OpenPracticePilotTiming.PilotKey(sessionId, pilotId)).ConfigureAwait(false) ?? throw new ArgumentException($"Session with ID {sessionId} does not exist.", nameof(sessionId));
        session.ResetMaximumLapTime();
        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.PilotKey>(session).ConfigureAwait(false);
    });
}
