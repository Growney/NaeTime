using System.Net;

namespace NaeTime.Command.Abstractions;
public interface IImmersionRCLapRFCommandHandler
{
    public Task RegisterNetworkLapRF8Channel(Guid id, string name, IPAddress address, ushort port);
    public Task ReconfigureNetworkDevice(Guid id, IPAddress address, ushort port);
    public Task RenameDevice(Guid id, string name);
    public Task SetupLane(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, float threshold, ushort gain);
    public Task ConfirmLaneSetup(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, float threshold, ushort gain);

    public Task RequestLaneStatus(Guid id, byte lane, bool isEnabled);
    public Task ConfirmLaneStatus(Guid id, byte lane, bool isEnabled);

    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz);
    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz);
    public Task RequestLaneThreshold(Guid id, byte lane, float threshold);
    public Task ConfirmLaneThresholdConfigured(Guid id, byte lane, float threshold);
    public Task RequestLaneGain(Guid id, byte lane, ushort gain);
    public Task ConfirmLaneGainConfigured(Guid id, byte lane, ushort gain);
    public Task MarkAsConnected(Guid id);
    public Task MarkAsDisconnected(Guid id);
}
