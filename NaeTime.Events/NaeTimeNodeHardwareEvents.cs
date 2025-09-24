namespace NaeTime.Events;

public record NaeTimeNodeSerialEsp32NodeConfigured(Guid TimerId, string Name, string Port, byte Lanes);
public record NaeTimeNodeSerialEsp32ConfigurationChanged(Guid TimerId, string Port);

public record NaeTimeNodeRenamed(Guid TimerId, string Name);
public record NaeTimeNodeLaneEnableRequested(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneDisableRequested(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneEnabled(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneDisabled(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneFrequencyRequested(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record NaeTimeNodeLaneFrequencyTuned(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record NaeTimeNodeLaneEntryThresholdRequested(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneEntryThresholdConfigured(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneExitThresholdRequested(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneExitThresholdConfigured(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneRFSetupRead(Guid TimerId, byte Lane, bool IsEnabled, byte? BandId, int FrequencyInMHz, ushort EntryThreshold, ushort ExitThreshold);
public record NaeTimeNodeLaneRFSetupConfirmationRequested(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneRFSetupConfirmed(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneRFSetupMismatch(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneRFSetupSyncEnabled(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneRFSetupSyncDisabled(Guid TimerId, byte Lane);
public record NaeTimeNodeTimerRFSetupConfirmationRequested(Guid TimerId);
public record NaeTimeNodeTimerRFSetupConfirmed(Guid TimerId);
public record NaeTimeNodeTimerRFSetupMismatch(Guid TimerId);
public record NaeTimeNodeTimerConnected(Guid TimerId);
public record NaeTimeNodeTimerDisconnected(Guid TimerId);