using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class NaeTimeNodeCommandHandler(IAggregateRepository repository) : INaeTimeNodeCommandHandler
{
    private readonly IAggregateRepository _repository = repository;

    public  Task ConfigureSerialEsp32Node(Guid id, string name, string port, byte lanes) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = _repository.CreateNew<NaeTimeNode>(() => new NaeTimeNode(id, name, port, lanes));
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestEnableLane(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestEnableLane(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestDisableLane(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestDisableLane(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneEnabled(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneEnabled(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneDisabled(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneDisabled(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneFrequency(lane, bandId, frequencyInMHz);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneFrequencyTuned(lane, bandId, frequencyInMHz);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneEntryThreshold(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneEntryThreshold(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneEntryThresholdConfigured(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneEntryThresholdConfigured(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestLaneExitThreshold(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneExitThreshold(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task ConfirmLaneExitThresholdConfigured(Guid id, byte lane, ushort threshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneExitThresholdConfigured(lane, threshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task MarkLaneRFSetupRead(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, ushort entryThreshold, ushort exitThreshold) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkLaneRFSetupRead(lane, isEnabled, bandId, frequencyInMHz, entryThreshold, exitThreshold);
        await _repository.Save<NaeTimeNode, Guid>(aggregate);
    });

    public Task RequestLaneRFSetupConfirmation(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestLaneRFSetupConfirmation(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task MarkLaneRFSetupConfirmed(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmLaneRFSetup(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task MarkLaneRFSetupMismatch(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkLaneRFSetupMismatch(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task EnableLaneRFSetupSync(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.EnableLaneRFSetupSync(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task DisableLaneRFSetupSync(Guid id, byte lane) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.DisableLaneRFSetupSync(lane);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RequestTimerRFSetupConfirmation(Guid id) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RequestTimerRFSetupConfirmation();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task MarkTimerRFSetupConfirmed(Guid id) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfirmTimerRFSetup();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task MarkTimerRFSetupMismatch(Guid id) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.MarkTimerRFSetupMismatch();
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
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

    public Task ReconfigureSerialNode(Guid id, string port) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ReconfigureSerialNode(port);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });

    public Task RenameDevice(Guid id, string name) => ConcurrencyException.Retry(async () =>
    {
        NaeTimeNode aggregate = await _repository.Get<NaeTimeNode, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.RenameDevice(name);
        await _repository.Save<NaeTimeNode, Guid>(aggregate).ConfigureAwait(false);
    });
}
