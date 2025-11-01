using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class NaeTimeNodeCommandHandler(IAggregateRepository repository) : INaeTimeNodeCommandHandler
{
    private readonly IAggregateRepository _repository = repository;

    public async Task ConfigureSerialEsp32Node(Guid id, string name, string port, byte lanes)
    {
        NaeTimeNode aggregate = _repository.CreateNew<NaeTimeNode>(() => new NaeTimeNode(id, name, port, lanes));
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestEnableLane(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestEnableLane(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestDisableLane(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestDisableLane(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneEnabled(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneEnabled(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneDisabled(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneDisabled(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneFrequency(lane, bandId, frequencyInMHz);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneFrequencyTuned(lane, bandId, frequencyInMHz);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestLaneEntryThreshold(Guid id, byte lane, ushort threshold)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneEntryThreshold(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneEntryThresholdConfigured(Guid id, byte lane, ushort threshold)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneEntryThresholdConfigured(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestLaneExitThreshold(Guid id, byte lane, ushort threshold)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneExitThreshold(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task ConfirmLaneExitThresholdConfigured(Guid id, byte lane, ushort threshold)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneExitThresholdConfigured(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkLaneRFSetupRead(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, ushort entryThreshold, ushort exitThreshold)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkLaneRFSetupRead(lane, isEnabled, bandId, frequencyInMHz, entryThreshold, exitThreshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate);
    }

    public async Task RequestLaneRFSetupConfirmation(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneRFSetupConfirmation(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkLaneRFSetupConfirmed(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneRFSetup(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkLaneRFSetupMismatch(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkLaneRFSetupMismatch(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task EnableLaneRFSetupSync(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.EnableLaneRFSetupSync(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task DisableLaneRFSetupSync(Guid id, byte lane)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.DisableLaneRFSetupSync(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RequestTimerRFSetupConfirmation(Guid id)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestTimerRFSetupConfirmation();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkTimerRFSetupConfirmed(Guid id)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmTimerRFSetup();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkTimerRFSetupMismatch(Guid id)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkTimerRFSetupMismatch();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkAsConnected(Guid id)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsConnected();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task MarkAsDisconnected(Guid id)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkAsDisconnected();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task ReconfigureSerialNode(Guid id, string port)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ReconfigureSerialNode(port);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }

    public async Task RenameDevice(Guid id, string name)
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RenameDevice(name);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    }
}
