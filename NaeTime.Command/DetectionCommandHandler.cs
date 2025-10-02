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

    public async Task AssignDetectionToSession(Guid DetectionId, Guid SessionId)
    {
        Detection? detection = await _repository.Get<Detection>(DetectionId) ?? throw new InvalidOperationException($"Detection {DetectionId} does not exist.");
        detection.AssignToSession(DetectionId, SessionId);
        await _repository.Save(detection);
    }

    public Task RegisterDetection(Guid DetectionId, byte Lane, long SoftwareTime, DateTime UtcTime)
    {
        Detection? detection = _repository.CreateNew<Detection>(() => new(DetectionId, Lane, SoftwareTime, UtcTime));
        return _repository.Save(detection);
    }

    public Task RegisterHardwareDetection(Guid DetectionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime)
    {
        Detection? detection = _repository.CreateNew<Detection>(() => new(DetectionId, TimerId, Lane, HardwareTime, SoftwareTime, UtcTime));
        return _repository.Save(detection);
    }

    public async Task UnassignDetectionFromSession(Guid DetectionId, Guid SessionId)
    {
        Detection? detection = await _repository.Get<Detection>(DetectionId) ?? throw new InvalidOperationException($"Detection {DetectionId} does not exist.");
        detection.UnassignFromSession(DetectionId, SessionId);
        await _repository.Save(detection);
    }
}
