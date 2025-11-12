using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
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
            Query.Abstractions.Models.Detector? detector = await _hardwareQueryHandler.GetDetector(detectorId).ConfigureAwait(false) ?? throw new ArgumentException($"Detector with ID {detectorId} does not exist.", nameof(detectors));
        }
    }

    public Task DesignTrack(Guid id, string name, Guid[] detectors) => ConcurrencyException.Retry(async () =>
    {
        await ThrowIfDetectorDoesNotExist(detectors).ConfigureAwait(false);

        Track track = _repository.CreateNew<Track>(() => new Track(id, detectors, name));

        await _repository.Save<Track, Guid>(track).ConfigureAwait(false);
    });

    public Task RenameTrack(Guid id, string name) => ConcurrencyException.Retry(async () =>
    {
        Track? track = await _repository.Get<Track, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException($"Track with ID {id} does not exist.", nameof(id));
        track.Rename(name);
        await _repository.Save<Track, Guid>(track).ConfigureAwait(false);
    });

    public Task ReorderTrackDetectors(Guid trackId, Guid[] detectors) => ConcurrencyException.Retry(async () =>
    {
        Track? track = await _repository.Get<Track, Guid>(trackId).ConfigureAwait(false) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        await ThrowIfDetectorDoesNotExist(detectors).ConfigureAwait(false);

        track.ReorderDetectors(detectors);
        await _repository.Save<Track, Guid>(track).ConfigureAwait(false);
    });

    public Task SetMaximumLapTime(Guid trackId, long milliseconds) => ConcurrencyException.Retry(async () =>
    {
        Track? track = await _repository.Get<Track, Guid>(trackId).ConfigureAwait(false) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetMaximumLapTime(milliseconds);
        await _repository.Save<Track, Guid>(track).ConfigureAwait(false);
    });

    public Task SetMinimumDetectionDelay(Guid trackId, long milliseconds) => ConcurrencyException.Retry(async () =>
    {
        Track? track = await _repository.Get<Track, Guid>(trackId).ConfigureAwait(false) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetMinimumDetectionDelay(milliseconds);
        await _repository.Save<Track, Guid>(track).ConfigureAwait(false);
    });

    public Task ResetMaximumLapTime(Guid trackId) => ConcurrencyException.Retry(async () =>
    {
        Track? track = await _repository.Get<Track, Guid>(trackId).ConfigureAwait(false) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetMaximumLapTime();
        await _repository.Save<Track, Guid>(track).ConfigureAwait(false);
    });

    public Task ResetMinimumDetectionDelay(Guid trackId) => ConcurrencyException.Retry(async () =>
    {
        Track? track = await _repository.Get<Track, Guid>(trackId).ConfigureAwait(false) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetMinimumDetectionDelay();
        await _repository.Save<Track, Guid>(track).ConfigureAwait(false);
    });
}
