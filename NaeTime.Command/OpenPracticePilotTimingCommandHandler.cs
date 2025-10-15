using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class OpenPracticePilotTimingCommandHandler(IAggregateRepository repository) : IOpenPracticePilotTimingCommandHandler
{
    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, byte ordinalPosition, byte detectorCount, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        OpenPracticePilotTiming.OpenPracticeTimingId id = new()
        {
            PilotId = pilotId,
            SessionId = sessionId
        };
        OpenPracticePilotTiming aggregate = await _repository.Get<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(id)
            ?? _repository.CreateNew<OpenPracticePilotTiming>(() => new(sessionId, pilotId));

        aggregate.AddDetectionToPilot(pilotId, sessionId, detectionId, ordinalPosition, hardwareTime, softwareTime, utcTime);

        await _repository.Save<OpenPracticePilotTiming, OpenPracticePilotTiming.OpenPracticeTimingId>(aggregate);
    }
}
