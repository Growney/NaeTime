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
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneStatus(lane, isEnabled);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneVideoFrequencyTuned(lane, bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task ConfirmLaneGainConfigured(Guid id, byte lane, ushort gain)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneGainConfigured(lane, gain);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task ConfirmLaneThresholdConfigured(Guid id, byte lane, float threshold)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmThresholdConfigured(lane, (int)threshold);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task MarkAsConnected(Guid id)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsConnected();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task MarkAsDisconnected(Guid id)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsDisconnected();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task ReconfigureNetworkDevice(Guid id, IPAddress address, ushort port)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfigureNetwork(address, port);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RegisterNetworkLapRF8Channel(Guid id, string name, IPAddress address, ushort port)
    {
        ImmersionRCLapRF aggregate = _repository.CreateNew<ImmersionRCLapRF>(() => new ImmersionRCLapRF(id, name, address, port, 8));
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RenameDevice(Guid id, string name)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.Rename(name);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RequestLaneStatus(Guid id, byte lane, bool isEnabled)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneStatus(lane, isEnabled);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestTuneLaneVideoFrequency(lane, bandId, frequencyInMHz);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RequestLaneGain(Guid id, byte lane, ushort gain)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneGain(lane, gain);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RequestLaneThreshold(Guid id, byte lane, float threshold)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneThreshold(lane, (int)threshold);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task SetupLane(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, float threshold, ushort gain)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneStatus(lane, isEnabled);
        aggregate.RequestTuneLaneVideoFrequency(lane, bandId, frequencyInMHz);
        aggregate.RequestLaneThreshold(lane, (int)threshold);
        aggregate.RequestLaneGain(lane, gain);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task ConfirmLaneSetup(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, float threshold, ushort gain)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneStatus(lane, isEnabled);
        aggregate.ConfirmLaneVideoFrequencyTuned(lane, bandId, frequencyInMHz);
        aggregate.ConfirmThresholdConfigured(lane, (int)threshold);
        aggregate.ConfirmLaneGainConfigured(lane, gain);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }
}
