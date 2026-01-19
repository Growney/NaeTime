using System.Net;

namespace NaeTime.Command.Abstractions;
public interface INaeTimeNodeCommandHandler
{
    public Task RegisterSerialEsp32Node(Guid id, string name, string port, byte lanes);
    public Task ChangeSerialEsp32Configuration(Guid id, string port);

    public Task RegisterNetworkNode(Guid id, string name, IPAddress address, ushort port, byte lanes);
    public Task ReconfigureNetworkDevice(Guid id, IPAddress address, ushort port);

    public Task RenameDevice(Guid id, string name);

    public Task SetupLaneForSession(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz);

    public Task RequestLaneStatus(Guid id, byte lane, bool isEnabled);
    public Task ConfirmLaneStatus(Guid id, byte lane, bool isEnabled);

    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz);
    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz);

    public Task RequestLaneEntryThreshold(Guid id, byte lane, ushort threshold);
    public Task ConfirmLaneEntryThreshold(Guid id, byte lane, ushort threshold);

    public Task RequestLaneExitThreshold(Guid id, byte lane, ushort threshold);
    public Task ConfirmLaneExitThreshold(Guid id, byte lane, ushort threshold);

    public Task MarkAsConnected(Guid id);
    public Task MarkAsDisconnected(Guid id);
    public Task ConfirmLaneSetup(Guid timerId, byte laneId, bool isEnabled, byte? bandId, int frequencyInMHz, ushort entryThreshold, ushort exitThreshold);
}
