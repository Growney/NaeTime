using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class NaeTimeNodeCommandHandler(IAggregateRepository repository) : INaeTimeNodeCommandHandler
{
    private readonly IAggregateRepository _repository = repository;

    public Task RegisterSerialEsp32Node(Guid id, string name, string port, byte lanes) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = _repository.CreateNew<NaeTimeNode>(() => new NaeTimeNode(id, name, port, lanes));
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ChangeSerialEsp32Configuration(Guid id, string port) => ConcurrencyException.Retry(async () =>
    {
        // reuse existing reconfigure serial implementation
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ReconfigureSerialNode(port);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RegisterNetworkNode(Guid id, string name, System.Net.IPAddress address, ushort port, byte lanes) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = _repository.CreateNew<NaeTimeNode>(() => new NaeTimeNode(id, name,address, port, lanes));
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ReconfigureNetworkDevice(Guid id, System.Net.IPAddress address, ushort port) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ReconfigureNetworkNode(address,port);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneStatus(Guid id, byte lane, bool isEnabled) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.RequestLaneStatus(isEnabled);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneStatus(Guid id, byte lane, bool isEnabled) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.ConfirmLaneStatus(isEnabled);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.RequestLaneFrequency(bandId, frequencyInMHz);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.ConfirmLaneFrequencyTuned(bandId, frequencyInMHz);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task RequestLaneEntryThreshold(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.RequestLaneEntryThreshold(threshold);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneEntryThreshold(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.ConfirmLaneEntryThresholdConfigured(threshold);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task RequestLaneExitThreshold(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.RequestLaneExitThreshold(threshold);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneExitThreshold(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNodeLane laneAggregate = await _repository.Get<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(new NaeTimeNodeLane.NaeTimeNodeLaneId(id, lane)).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        laneAggregate.ConfirmLaneExitThresholdConfigured(threshold);
        await _repository.Save<NaeTimeNodeLane, NaeTimeNodeLane.NaeTimeNodeLaneId>(laneAggregate).ConfigureAwait(false);
    });

    public Task MarkAsConnected(Guid id) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsConnected();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task MarkAsDisconnected(Guid id) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsDisconnected();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RenameDevice(Guid id, string name) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RenameDevice(name);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });
}
