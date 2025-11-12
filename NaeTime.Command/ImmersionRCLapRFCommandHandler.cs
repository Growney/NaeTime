using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using System.Net;

namespace NaeTime.Command;
public class ImmersionRCLapRFCommandHandler(IAggregateRepository repository) : IImmersionRCLapRFCommandHandler
{
    private readonly IAggregateRepository _repository = repository;

    public Task ConfirmLaneStatus(Guid id, byte lane, bool isEnabled) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneStatus(isEnabled);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneVideoFrequencyTuned(bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneGainConfigured(Guid id, byte lane, ushort gain) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneGainConfigured(gain);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneThresholdConfigured(Guid id, byte lane, float threshold) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmThresholdConfigured(threshold);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task MarkAsConnected(Guid id) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsConnected();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task MarkAsDisconnected(Guid id) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsDisconnected();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ReconfigureNetworkDevice(Guid id, IPAddress address, ushort port) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfigureNetwork(address, port);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    });

    public async Task RegisterNetworkLapRF8Channel(Guid id, string name, IPAddress address, ushort port)
    {
        byte lanes = 8;

        for (byte i = 0; i < lanes; i++)
        {
            ImmersionRCLapRFLane laneAggregate = _repository.CreateNew<ImmersionRCLapRFLane>(() => new ImmersionRCLapRFLane(id, i));
            await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(laneAggregate).ConfigureAwait(false);
        }

        ImmersionRCLapRF aggregate = _repository.CreateNew<ImmersionRCLapRF>(() => new ImmersionRCLapRF(id, name, address, port, lanes));
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    }

    public Task RenameDevice(Guid id, string name) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.Rename(name);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneStatus(Guid id, byte lane, bool isEnabled) => ConcurrencyException.Retry(async () => 
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneStatus(isEnabled);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestTuneLaneVideoFrequency(bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneGain(Guid id, byte lane, ushort gain) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneGain(gain);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneThreshold(Guid id, byte lane, float threshold) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneThreshold(threshold);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task SetupLaneForSession(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneStatus(isEnabled);
        aggregate.RequestTuneLaneVideoFrequency(bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneSetup(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, float threshold, ushort gain) => ConcurrencyException.Retry(async () =>
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneStatus(isEnabled);
        aggregate.ConfirmLaneVideoFrequencyTuned(bandId, frequencyInMHz);
        aggregate.ConfirmThresholdConfigured((int)threshold);
        aggregate.ConfirmLaneGainConfigured(gain);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    });
}
