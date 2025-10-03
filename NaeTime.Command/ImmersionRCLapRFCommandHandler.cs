using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using System.Net;

namespace NaeTime.Command;
public class ImmersionRCLapRFCommandHandler : IImmersionRCLapRFCommandHandler
{
    private readonly IAggregateRepository _repository;

    public ImmersionRCLapRFCommandHandler(IAggregateRepository repository)
    {
        _repository = repository;
    }

    public async Task ConfirmLaneDisabled(Guid id, byte lane)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneDisabled(lane);
        await _repository.Save<ImmersionRCLapRF,Guid>(aggregate);
    }

    public async Task ConfirmLaneEnabled(Guid id, byte lane)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneEnabled(lane);
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

    public async Task DisableRFSetupSync(Guid id)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.DisableRFSetupSync();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task EnableRFSetupSync(Guid id)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.EnableRFSetupSync();
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

    public async Task MarkRFSetupConfirmed(Guid id, byte lane)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmConfiguration();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task MarkRFSetupMismatch(Guid id, byte lane)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkConfigurationMismatch();
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
        ImmersionRCLapRF aggregate = _repository.CreateNew<ImmersionRCLapRF, Guid>(() => new ImmersionRCLapRF(id, name, address, port, 8));
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RenameDevice(Guid id, string name)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.Rename(name);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RequestDisableLane(Guid id, byte lane)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestDisableLane(lane);
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }

    public async Task RequestEnableLane(Guid id, byte lane)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestEnableLane(lane);
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

    public async Task RequestRFSetupConfirmation(Guid id, byte lane)
    {
        ImmersionRCLapRF aggregate = await _repository.Get<ImmersionRCLapRF, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestConfigurationConfirmation();
        await _repository.Save<ImmersionRCLapRF, Guid>(aggregate);
    }
}
