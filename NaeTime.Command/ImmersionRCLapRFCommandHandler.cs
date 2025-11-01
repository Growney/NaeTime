using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using System.Net;

namespace NaeTime.Command;
public class ImmersionRCLapRFCommandHandler(IAggregateRepository repository) : IImmersionRCLapRFCommandHandler
{
    private readonly IAggregateRepository _repository = repository;


    public async Task ConfirmLaneStatus(Guid id, byte lane, bool isEnabled)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneStatus(isEnabled);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneVideoFrequencyTuned(bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneGainConfigured(Guid id, byte lane, ushort gain)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneGainConfigured(gain);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneThresholdConfigured(Guid id, byte lane, float threshold)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmThresholdConfigured(threshold);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkAsConnected(Guid id)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsConnected();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkAsDisconnected(Guid id)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsDisconnected();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task ReconfigureNetworkDevice(Guid id, IPAddress address, ushort port)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfigureNetwork(address, port);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    }

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

    public async Task RenameDevice(Guid id, string name)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.Rename(name);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestLaneStatus(Guid id, byte lane, bool isEnabled)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneStatus(isEnabled);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestTuneLaneVideoFrequency(bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestLaneGain(Guid id, byte lane, ushort gain)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneGain(gain);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestLaneThreshold(Guid id, byte lane, float threshold)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneThreshold(threshold);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task SetupLaneForSession(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneStatus(isEnabled);
        aggregate.RequestTuneLaneVideoFrequency(bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneSetup(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, float threshold, ushort gain)
    {
        ImmersionRCLapRFLane aggregate = await _repository.Get<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(new ImmersionRCLapRFLane.ImmersionRCLapRFLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneStatus(isEnabled);
        aggregate.ConfirmLaneVideoFrequencyTuned(bandId, frequencyInMHz);
        aggregate.ConfirmThresholdConfigured((int)threshold);
        aggregate.ConfirmLaneGainConfigured(gain);
        await _repository.Save<ImmersionRCLapRFLane, ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>(aggregate).ConfigureAwait(false);
    }
}
