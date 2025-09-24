namespace NaeTime.Command.Abstractions;
public interface INaeTimeNodeCommandHandler
{
    Task ConfigureSerialEsp32Node(Guid id, string name, string port, byte lanes);
    Task ReconfigureSerialNode(Guid id, string port);
    Task RenameDevice(Guid id, string name);
    Task RequestEnableLane(Guid id, byte lane);
    Task RequestDisableLane(Guid id, byte lane);
    Task ConfirmLaneEnabled(Guid id, byte lane);
    Task ConfirmLaneDisabled(Guid id, byte lane);
    Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz);
    Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz);
    Task RequestLaneEntryThreshold(Guid id, byte lane, ushort threshold);
    Task ConfirmLaneEntryThresholdConfigured(Guid id, byte lane, ushort threshold);
    Task RequestLaneExitThreshold(Guid id, byte lane, ushort threshold);
    Task ConfirmLaneExitThresholdConfigured(Guid id, byte lane, ushort threshold);
    Task MarkLaneRFSetupRead(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, ushort entryThreshold, ushort exitThreshold);
    Task RequestLaneRFSetupConfirmation(Guid id, byte lane);
    Task MarkLaneRFSetupConfirmed(Guid id, byte lane);
    Task MarkLaneRFSetupMismatch(Guid id, byte lane);
    Task EnableLaneRFSetupSync(Guid id, byte lane);
    Task DisableLaneRFSetupSync(Guid id, byte lane);
    Task RequestTimerRFSetupConfirmation(Guid id);
    Task MarkTimerRFSetupConfirmed(Guid id);
    Task MarkTimerRFSetupMismatch(Guid id);
    Task MarkAsConnected(Guid id);
    Task MarkAsDisconnected(Guid id);
}
