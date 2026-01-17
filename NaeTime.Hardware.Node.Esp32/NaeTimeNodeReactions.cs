using EventDbLite.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Hardware.Abstractions;
using System;
using System.Threading.Tasks;

namespace NaeTime.Hardware.Node.Esp32;
internal class NaeTimeNodeReactions
{
    private readonly INodeConnectionProvider _connectionProvider;
    private readonly INaeTimeNodeCommandHandler _commandHandler;
    private readonly IStreamEventWriter _writer;

    public NaeTimeNodeReactions(INodeConnectionProvider connectionProvider, INaeTimeNodeCommandHandler commandHandler, IStreamEventWriter writer)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    private Task When(NaeTimeNodeLaneFrequencyRequested requested)
        => HandleTuneRequest(requested.TimerId, requested.Lane, requested.BandId, requested.FrequencyInMHz);

    private Task When(NaeTimeNodeLaneFrequencyMismatch mismatch)
        => HandleTuneRequest(mismatch.TimerId, mismatch.Lane, mismatch.DesiredBandId, mismatch.DesiredFrequencyInMHz);

    private Task When(NaeTimeNodeLaneEnableRequested requested)
        => HandleStatusRequest(requested.TimerId, requested.Lane, true);

    private Task When(NaeTimeNodeLaneDisableRequested requested)
        => HandleStatusRequest(requested.TimerId, requested.Lane, false);

    private Task When(NaeTimeNodeLaneStatusMismatch mismatch)
        => HandleStatusRequest(mismatch.TimerId, mismatch.Lane, mismatch.DesiredStatus);

    private Task When(NaeTimeNodeLaneEntryThresholdRequested requested)
        => HandleEntryThresholdRequested(requested.TimerId, requested.Lane, requested.Threshold);

    private Task When(NaeTimeNodeLaneExitThresholdRequested requested)
        => HandleExitThresholdRequested(requested.TimerId, requested.Lane, requested.Threshold);

    private async Task HandleTuneRequest(Guid timerId, byte lane, byte? bandId, int frequencyInMhz)
    {
        INodeConnection? connection = _connectionProvider.GetNodeConnection(timerId);
        if (connection == null)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeLaneFrequencyTuningFailed(timerId, lane, bandId, frequencyInMhz, "No connection found for the specified TimerId")).ConfigureAwait(false);
            return;
        }

        if (!connection.IsConnected)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeLaneFrequencyTuningFailed(timerId, lane, bandId, frequencyInMhz, "Connection is not established")).ConfigureAwait(false);
            return;
        }

        await connection.SetLaneRadioFrequency(lane, frequencyInMhz).ConfigureAwait(false);

        // We don't have a read-back API for node connection; assume success and confirm
        await _commandHandler.ConfirmLaneFrequencyTuned(timerId, lane, bandId, frequencyInMhz).ConfigureAwait(false);
    }

    private async Task HandleStatusRequest(Guid timerId, byte lane, bool isEnabled)
    {
        INodeConnection? connection = _connectionProvider.GetNodeConnection(timerId);
        if (connection == null)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeRFLaneStatusConfigurationFailed(timerId, lane, isEnabled, "No connection found for the specified TimerId")).ConfigureAwait(false);
            return;
        }

        if (!connection.IsConnected)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeRFLaneStatusConfigurationFailed(timerId, lane, isEnabled, "Connection is not established")).ConfigureAwait(false);
            return;
        }

        await connection.SetLaneEnabled(lane, isEnabled).ConfigureAwait(false);

        // Assume success
        await _commandHandler.ConfirmLaneStatus(timerId, lane, isEnabled).ConfigureAwait(false);
    }

    private async Task HandleEntryThresholdRequested(Guid timerId, byte lane, ushort threshold)
    {
        INodeConnection? connection = _connectionProvider.GetNodeConnection(timerId);
        if (connection == null)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeLaneEntryThresholdConfigurationFailed(timerId, lane, threshold, "No connection found for the specified TimerId")).ConfigureAwait(false);
            return;
        }

        if (!connection.IsConnected)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeLaneEntryThresholdConfigurationFailed(timerId, lane, threshold, "Connection is not established")).ConfigureAwait(false);
            return;
        }

        await connection.SetLaneEntryThreshold(lane, threshold).ConfigureAwait(false);

        await _commandHandler.ConfirmLaneEntryThreshold(timerId, lane, threshold).ConfigureAwait(false);
    }

    private async Task HandleExitThresholdRequested(Guid timerId, byte lane, ushort threshold)
    {
        INodeConnection? connection = _connectionProvider.GetNodeConnection(timerId);
        if (connection == null)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeLaneExitThresholdConfigurationFailed(timerId, lane, threshold, "No connection found for the specified TimerId")).ConfigureAwait(false);
            return;
        }

        if (!connection.IsConnected)
        {
            await _writer.AppendToStream($"Node_Errors_{timerId}", new NaeTimeNodeLaneExitThresholdConfigurationFailed(timerId, lane, threshold, "Connection is not established")).ConfigureAwait(false);
            return;
        }

        await connection.SetLaneExitThreshold(lane, threshold).ConfigureAwait(false);

        await _commandHandler.ConfirmLaneExitThreshold(timerId, lane, threshold).ConfigureAwait(false);
    }
}
