using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Command;
public class OpenPracticePilotTimingCommandHandler(IAggregateRepository repository) : IOpenPracticePilotTimingCommandHandler
{
    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        OpenPracticePilotTiming.OpenPracticeTimingId id = new ()
        {
            PilotId = pilotId,
            SessionId = sessionId
        };
        OpenPracticePilotTiming aggregate = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(id) 
            ?? _repository.CreateNew<OpenPracticePilotTiming>(() => new(sessionId, pilotId));

        aggregate.AddDetectionToPilot(pilotId, sessionId, detectionId, hardwareTime, softwareTime, utcTime);

        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(aggregate);
    }

    public async Task MarkPilotDetectionAsInvalid(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        OpenPracticePilotTiming.OpenPracticeTimingId id = new()
        {
            PilotId = pilotId,
            SessionId = sessionId
        };
        OpenPracticePilotTiming aggregate = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(id)
            ?? _repository.CreateNew<OpenPracticePilotTiming>(() => new(sessionId,pilotId));

        aggregate.MarkPilotDetectionAsInvalid(detectionId);

        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(aggregate);
    }

    public async Task MarkPilotDetectionAsValid(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        OpenPracticePilotTiming.OpenPracticeTimingId id = new()
        {
            PilotId = pilotId,
            SessionId = sessionId
        };

        OpenPracticePilotTiming aggregate = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(id)
            ?? _repository.CreateNew<OpenPracticePilotTiming>(() => new(sessionId, pilotId));

        aggregate.MarkPilotDetectionAsValid(detectionId);

        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId> (aggregate);
    }

    public async Task RemoveDetectionFromPilot(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        OpenPracticePilotTiming.OpenPracticeTimingId id = new()
        {
            PilotId = pilotId,
            SessionId = sessionId
        };

        OpenPracticePilotTiming aggregate = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(id)
            ?? _repository.CreateNew<OpenPracticePilotTiming>(() => new(sessionId, pilotId));

        aggregate.RemoveDetectionFromPilot(detectionId);

        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(aggregate);
    }
}
