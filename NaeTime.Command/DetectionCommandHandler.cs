using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class DetectionCommandHandler : IDetectionCommandHandler
{
    private readonly IAggregateRepository _repository;

    public DetectionCommandHandler(IAggregateRepository repository)
    {
        _repository = repository;
    }

    public async Task BindDetectionToOpenPracticeSession(Guid detectionId, Guid sessionId)
    {
        Detection? detection = await _repository.Get<Detection, Guid>(detectionId) ?? throw new InvalidOperationException($"Detection {detectionId} does not exist.");
        detection.MarkDetectionWithinOpenPracticeBounds(detectionId, sessionId);
        await _repository.Save<Detection, Guid>(detection);
    }

    public Task RegisterDetection(Guid detectionId, byte lane, long softwareTime, DateTime utcTime)
    {
        Detection? detection = _repository.CreateNew<Detection, Guid>(() => new(detectionId, lane, softwareTime, utcTime));
        return _repository.Save<Detection, Guid>(detection);
    }

    public Task RegisterHardwareDetection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Detection? detection = _repository.CreateNew<Detection, Guid>(() => new(detectionId, timerId, lane, hardwareTime, softwareTime, utcTime));
        return _repository.Save<Detection, Guid>(detection);
    }

    public async Task UnbindDetectionFromOpenPracticeSession(Guid detectionId, Guid sessionId)
    {
        Detection? detection = await _repository.Get<Detection, Guid>(detectionId) ?? throw new InvalidOperationException($"Detection {detectionId} does not exist.");
        detection.UnassignFromSession(detectionId, sessionId);
        await _repository.Save<Detection, Guid>(detection);
    }
}
