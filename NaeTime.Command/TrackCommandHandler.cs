using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;

namespace NaeTime.Command;
public class TrackCommandHandler(IAggregateRepository repository, IHardwareQueryHandler hardwareQueryHandler) : ITrackCommandHandler
{
    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IHardwareQueryHandler _hardwareQueryHandler = hardwareQueryHandler ?? throw new ArgumentNullException(nameof(hardwareQueryHandler));

    private async Task ThrowIfDetectorDoesNotExist(IEnumerable<Guid> detectors)
    {
        foreach (Guid detectorId in detectors)
        {
            Query.Abstractions.Models.Detector? detector = await _hardwareQueryHandler.GetDetector(detectorId) ?? throw new ArgumentException($"Detector with ID {detectorId} does not exist.", nameof(detectors));
        }
    }

    public async Task DesignTrack(Guid id, string name, Guid[] detectors)
    {
        await ThrowIfDetectorDoesNotExist(detectors);

        Track track = _repository.CreateNew<Track>(() => new Track(id, detectors, name));

        await _repository.Save<Track, Guid>(track);
    }

    public async Task RenameTrack(Guid id, string name)
    {
        Track? track = await _repository.Get<Track, Guid>(id) ?? throw new ArgumentException($"Track with ID {id} does not exist.", nameof(id));
        track.Rename(name);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task ReorderTrackDetectors(Guid trackId, Guid[] detectors)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        await ThrowIfDetectorDoesNotExist(detectors);

        track.ReorderDetectors(detectors);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task SetMaximumLapTime(Guid trackId, long milliseconds)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetMaximumLapTime(milliseconds);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task SetMinimumDetectionDelay(Guid trackId, long milliseconds)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetMinimumDetectionDelay(milliseconds);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task ResetMaximumLapTime(Guid trackId)
    {
        Track track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetMaximumLapTime();
        await _repository.Save<Track, Guid>(track);
    }

    public Task ResetMinimumDetectionDelay(Guid trackId)
    {
        Track track = _repository.Get<Track, Guid>(trackId).Result ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetMinimumDetectionDelay();
        return _repository.Save<Track, Guid>(track);
    }
}
