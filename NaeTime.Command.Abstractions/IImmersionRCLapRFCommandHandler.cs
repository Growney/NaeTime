using System.Net;

namespace NaeTime.Command.Abstractions;
public interface IImmersionRCLapRFCommandHandler
{
    public Task RegisterNetworkLapRF8Channel(Guid id, string name, IPAddress address, ushort port);
    public Task ReconfigureNetworkDevice(Guid id, IPAddress address, ushort port);
    public Task RenameDevice(Guid id, string name);
    public Task RequestEnableLane(Guid id, byte lane);
    public Task RequestDisableLane(Guid id, byte lane);
    public Task ConfirmLaneEnabled(Guid id, byte lane);
    public Task ConfirmLaneDisabled(Guid id, byte lane);
    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz);
    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz);
    public Task RequestLaneThreshold(Guid id, byte lane, float threshold);
    public Task ConfirmLaneThresholdConfigured(Guid id, byte lane, float threshold);
    public Task RequestLaneGain(Guid id, byte lane, ushort gain);
    public Task ConfirmLaneGainConfigured(Guid id, byte lane, ushort gain);
    public Task MarkAsConnected(Guid id);
    public Task MarkAsDisconnected(Guid id);
    public Task EnableRFSetupSync(Guid id);
    public Task DisableRFSetupSync(Guid id);
    public Task RequestRFSetupConfirmation(Guid id, byte lane);
    public Task MarkRFSetupConfirmed(Guid id, byte lane);
    public Task MarkRFSetupMismatch(Guid id, byte lane);

}
