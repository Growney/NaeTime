namespace NaeTime.Events;

public record NaeTimeNodeSerialEsp32NodeRegistered(Guid TimerId, string Name, string Port, byte Lanes);
public record NaeTimeNodeSerialEsp32NodeConfigurationChanged(Guid TimerId, string Port);

public record NaeTimeNodeNetworkDeviceRegistered(Guid TimerId, string Name, string IPAddress, ushort Port, byte Lanes);
public record NaeTimeNodeNetworkConfigurationChanged(Guid TimerId, string IPAddress, ushort Port);
public record NaeTimeNodeRFLaneCreated(Guid TimerId, byte LaneId);

public record NaeTimeNodeRenamed(Guid TimerId, string Name);

public record NaeTimeNodeSerialEsp32ConfigurationChanged(Guid TimerId, string Port);

public record NaeTimeNodeLaneEnableRequested(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneDisableRequested(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneEnabled(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneDisabled(Guid TimerId, byte Lane);
public record NaeTimeNodeLaneStatusMismatch(Guid TimerId, byte Lane, bool DesiredStatus, bool ActualStatus);
public record NaeTimeNodeRFLaneStatusConfigurationFailed(Guid TimerId, byte Lane, bool DesiredStatus, string Reason);

public record NaeTimeNodeLaneFrequencyRequested(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record NaeTimeNodeLaneFrequencyTuned(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record NaeTimeNodeLaneFrequencyMismatch(Guid TimerId, byte Lane, byte? DesiredBandId, int DesiredFrequencyInMHz, byte? ActualBandId, int ActualFrequencyInMHz);
public record NaeTimeNodeLaneFrequencyTuningFailed(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz, string Reason);

public record NaeTimeNodeLaneEntryThresholdRequested(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneEntryThresholdConfigured(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneExitThresholdMismatch(Guid TimerId, byte Lane, ushort DesiredThreshold, ushort ActualThreshold);
public record NaeTimeNodeLaneExitThresholdConfigurationFailed(Guid TimerId, byte Lane, ushort Threshold, string Reason);

public record NaeTimeNodeLaneExitThresholdRequested(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneExitThresholdConfigured(Guid TimerId, byte Lane, ushort Threshold);
public record NaeTimeNodeLaneEntryThresholdMismatch(Guid TimerId, byte Lane, ushort DesiredThreshold, ushort ActualThreshold);
public record NaeTimeNodeLaneEntryThresholdConfigurationFailed(Guid TimerId, byte Lane, ushort Threshold, string Reason);

public record NaeTimeNodeTimerConnected(Guid TimerId);
public record NaeTimeNodeTimerDisconnected(Guid TimerId);
public record NaeTimeNodeConfigurationUnconfirmed(Guid TimerId, byte Lanes);
public record NaeTimeNodeLaneConfigurationUnconfirmed(Guid TimerId, byte Lane);