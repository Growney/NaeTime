using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class DetectionCommandHandler(IAggregateRepository repository) : IDetectionCommandHandler
{
    private readonly IAggregateRepository _repository = repository;

    public async Task AssignDetectionToOpenPracticeSesssion(Guid detectionId, Guid sessionId)
    {
        HardwareDetection? detection = await _repository.Get<HardwareDetection, Guid>(detectionId) ?? throw new InvalidOperationException($"Detection {detectionId} does not exist.");
        detection.AssignDetectionToOpenPracticeSession(detectionId, sessionId);
        await _repository.Save<HardwareDetection, Guid>(detection);
    }

    public Task RegisterHardwareDetection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        HardwareDetection? detection = _repository.CreateNew<HardwareDetection>(() => new(detectionId, timerId, lane, hardwareTime, softwareTime, utcTime));
        return _repository.Save<HardwareDetection, Guid>(detection);
    }
}
