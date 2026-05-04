using EventDbLite.Abstractions;
using EventDbLite.Streams;
using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.ImmersionRC.Models;

namespace NaeTime.Hardware.ImmersionRC;
internal class ImmersionRCLapRFReactions
{
    private readonly ILapRFConnectionProvider _lapRFConnectionProvider;
    private readonly IImmersionRCLapRFCommandHandler _immersionRCLapRFCommandHandler;
    private readonly IStreamEventWriter _streamEventWriter;

    public ImmersionRCLapRFReactions(ILapRFConnectionProvider lapRFConnectionProvider, IImmersionRCLapRFCommandHandler immersionRCLapRFCommandHandler, IStreamEventWriter streamEventWriter)
    {
        _lapRFConnectionProvider = lapRFConnectionProvider;
        _immersionRCLapRFCommandHandler = immersionRCLapRFCommandHandler;
        _streamEventWriter = streamEventWriter;
    }

    private Task When(ImmersionRCLapRFLaneFrequencyRequested requested)
        => HandleTuneRequest(requested.TimerId, requested.Lane, requested.BandId, requested.FrequencyInMHz);
    private Task When(ImmersionRCLapRFLaneFrequencyMismatch mismatch)
        => HandleTuneRequest(mismatch.TimerId, mismatch.Lane, mismatch.DesiredBandId, mismatch.DesiredFrequencyInMHz);
    private Task When(ImmersionRCLapRFLaneEnableRequested requested)
        => HandleStatusRequest(requested.TimerId, requested.Lane, true);
    private Task When(ImmersionRCLapRFLaneDisableRequested requested)
        => HandleStatusRequest(requested.TimerId, requested.Lane, false);
    private Task When(ImmersionRCLapRFLaneStatusMismatch mismatch)
        => HandleStatusRequest(mismatch.TimerId, mismatch.Lane, mismatch.DesiredStatus);
    private Task When(ImmersionRCLapRFLaneThresholdRequested requested)
        => HandleThresholdRequested(requested.TimerId, requested.Lane, requested.Threshold);
    private Task When(ImmersionRCLapRFLaneThresholdMismatch mismatch)
        => HandleThresholdRequested(mismatch.TimerId, mismatch.Lane, mismatch.DesiredThreshold);
    private Task When(ImmersionRCLapRFLaneGainRequested requested)
        => HandleGainRequest(requested.TimerId, requested.Lane, requested.Gain);
    private Task When(ImmersionRCLapRFLaneGainMismatch mismatch)
        => HandleGainRequest(mismatch.TimerId, mismatch.Lane, mismatch.DesiredGain);

    private async Task When(ImmersionRCLapRFConfigurationUnconfirmed unconfirmed)
    {
        ILapRFConnection? connection = _lapRFConnectionProvider.GetLapRFConnection(unconfirmed.TimerId);
        if (connection == null)
        {
            return;
        }
        if (!connection.IsConnected)
        {
            return;
        }
        IEnumerable<LapRFLaneConfiguration> timerConfigurations = await connection.GetAllLaneConfigurations();
        foreach (LapRFLaneConfiguration laneConfiguration in timerConfigurations)
        {
            await _immersionRCLapRFCommandHandler.ConfirmLaneSetup(unconfirmed.TimerId, laneConfiguration.Lane, laneConfiguration.IsEnabled, laneConfiguration.BandId, laneConfiguration.FrequencyInMhz, laneConfiguration.Threshold, laneConfiguration.Gain);
        }
    }

    private async Task HandleTuneRequest(Guid timerId, byte lane, byte? bandId, int frequencyInMhz)
    {
        ILapRFConnection? connection = _lapRFConnectionProvider.GetLapRFConnection(timerId);
        if (connection == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneFrequencyTuningFailed(timerId, lane, bandId, frequencyInMhz, "No connection found for the specified TimerId"));
            return;
        }

        if (!connection.IsConnected)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneFrequencyTuningFailed(timerId, lane, bandId, frequencyInMhz, "Connection is not established"));
            return;
        }

        await connection.SetLaneRadioFrequency(lane, bandId, frequencyInMhz);
        IEnumerable<LapRFLaneConfiguration> timerConfigurations = await connection.GetLaneConfigurations(lane);
        LapRFLaneConfiguration? laneConfiguration = timerConfigurations.FirstOrDefault();
        if (laneConfiguration == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneFrequencyTuningFailed(timerId, lane, bandId, frequencyInMhz, "Lane configuration not found after setting frequency"));
            return;
        }
        await _immersionRCLapRFCommandHandler.ConfirmLaneFrequencyTuned(timerId, lane, laneConfiguration.BandId, laneConfiguration.FrequencyInMhz);
    }
    private async Task HandleStatusRequest(Guid timerId, byte lane, bool isEnabled)
    {
        ILapRFConnection? connection = _lapRFConnectionProvider.GetLapRFConnection(timerId);
        if (connection == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneStatusConfigurationFailed(timerId, lane, isEnabled, "No connection found for the specified TimerId"));
            return;
        }

        if (!connection.IsConnected)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneStatusConfigurationFailed(timerId, lane, isEnabled, "Connection is not established"));
            return;
        }

        await connection.SetLaneStatus(lane, isEnabled);
        IEnumerable<LapRFLaneConfiguration> timerConfigurations = await connection.GetLaneConfigurations(lane);
        LapRFLaneConfiguration? laneConfiguration = timerConfigurations.FirstOrDefault();
        if (laneConfiguration == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneStatusConfigurationFailed(timerId, lane, isEnabled, "Lane configuration not found after setting status"));
            return;
        }
        await _immersionRCLapRFCommandHandler.ConfirmLaneStatus(timerId, lane, laneConfiguration.IsEnabled);
    }
    private async Task HandleThresholdRequested(Guid timerId, byte lane, float threshold)
    {
        ILapRFConnection? connection = _lapRFConnectionProvider.GetLapRFConnection(timerId);
        if (connection == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneThresholdConfigurationFailed(timerId, lane, threshold, "No connection found for the specified TimerId"));
            return;
        }

        if (!connection.IsConnected)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneThresholdConfigurationFailed(timerId, lane, threshold, "Connection is not established"));
            return;
        }

        await connection.SetLaneThreshold(lane, threshold);
        IEnumerable<LapRFLaneConfiguration> timerConfigurations = await connection.GetLaneConfigurations(lane);
        LapRFLaneConfiguration? laneConfiguration = timerConfigurations.FirstOrDefault();
        if (laneConfiguration == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneThresholdConfigurationFailed(timerId, lane, threshold, "Lane configuration not found after setting threshold"));
            return;
        }
        await _immersionRCLapRFCommandHandler.ConfirmLaneThresholdConfigured(timerId, lane, laneConfiguration.Threshold);
    }
    private async Task HandleGainRequest(Guid timerId, byte lane, ushort gain)
    {
        ILapRFConnection? connection = _lapRFConnectionProvider.GetLapRFConnection(timerId);
        if (connection == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneGainConfigurationFailed(timerId, lane, gain, "No connection found for the specified TimerId"));
            return;
        }

        if (!connection.IsConnected)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneGainConfigurationFailed(timerId, lane, gain, "Connection is not established"));
            return;
        }

        await connection.SetLaneGain(lane, gain);
        IEnumerable<LapRFLaneConfiguration> timerConfigurations = await connection.GetLaneConfigurations(lane);
        LapRFLaneConfiguration? laneConfiguration = timerConfigurations.FirstOrDefault();
        if (laneConfiguration == null)
        {
            await _streamEventWriter.AppendToStream($"LapRF_Errors_{timerId}",
                new ImmersionRCLapRFLaneGainConfigurationFailed(timerId, lane, gain, "Lane configuration not found after setting gain"));
            return;
        }
        await _immersionRCLapRFCommandHandler.ConfirmLaneGainConfigured(timerId, lane, laneConfiguration.Gain);
    }
}
