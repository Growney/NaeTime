using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;

namespace NaeTime.Command;
public class TrackCommandHandler : ITrackCommandHandler
{
    private readonly IAggregateRepository _repository;
    private readonly IHardwareQueryHandler _hardwareQueryHandler;

    public TrackCommandHandler(IAggregateRepository repository, IHardwareQueryHandler hardwareQueryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _hardwareQueryHandler = hardwareQueryHandler ?? throw new ArgumentNullException(nameof(hardwareQueryHandler));
    }

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

        Track track = _repository.CreateNew<Track,Guid>(() => new Track(id, detectors, name));

        await _repository.Save<Track, Guid>(track);
    }

    public async Task RenameTrack(Guid id, string name)
    {
        Track? track = await _repository.Get<Track,Guid>(id) ?? throw new ArgumentException($"Track with ID {id} does not exist.", nameof(id));
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

    public async Task ResetPilotMaximumLapTime(Guid trackId, Guid pilotId)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetPilotMaximumTimeLapTime(pilotId);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task ResetPilotMinimumLapTime(Guid trackId, Guid pilotId)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetPilotMinimumTimeLapTime(pilotId);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task SetMaximumLapTime(Guid trackId, long milliseconds)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetMaximumLapTime(milliseconds);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task SetMinimumLapTime(Guid trackId, long milliseconds)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetMinimumLapTime(milliseconds);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task SetPilotMaximumLapTime(Guid trackId, Guid pilotId, long milliseconds)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetPilotMaximumLapTime(pilotId, milliseconds);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task SetPilotMinimumLapTime(Guid trackId, Guid pilotId, long milliseconds)
    {
        Track? track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.SetPilotMinimumLapTime(pilotId, milliseconds);
        await _repository.Save<Track, Guid>(track);
    }

    public async Task ResetMaximumLapTime(Guid trackId)
    {
        Track track = await _repository.Get<Track, Guid>(trackId) ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetMaximumLapTime();
        await _repository.Save<Track, Guid>(track);
    }

    public Task ResetMinimumLapTime(Guid trackId)
    {
        Track track = _repository.Get<Track, Guid>(trackId).Result ?? throw new ArgumentException($"Track with ID {trackId} does not exist.", nameof(trackId));
        track.ResetMinimumLapTime();
        return _repository.Save<Track, Guid>(track);
    }
}
