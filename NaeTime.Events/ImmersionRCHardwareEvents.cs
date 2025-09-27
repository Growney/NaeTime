namespace NaeTime.Events;

public record ImmersionRCLapRFNetworkDeviceRegistered(Guid TimerId, string IPAddress, ushort Port, byte Lanes);
public record ImmersionRCLapRFNetworkConfigurationChanged(Guid TimerId, string IPAddress, ushort Port);

public record ImmersionRCLapRFRenamed(Guid TimerId, string Name);
public record ImmersionRCLapRFLaneEnableRequested(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneDisableRequested(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneEnabled(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneDisabled(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneFrequencyRequested(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record ImmersionRCLapRFLaneFrequencyTuned(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record ImmersionRCLapRFLaneThresholdRequested(Guid TimerId, byte Lane, float Threshold);
public record ImmersionRCLapRFLaneThresholdConfigured(Guid TimerId, byte Lane, float Threshold);
public record ImmersionRCLapRFLaneGainRequested(Guid TimerId, byte Lane, ushort Gain);
public record ImmersionRCLapRFLaneGainConfigured(Guid TimerId, byte Lane, ushort Gain);
public record ImmersionRCLapRFLaneRFSetupConfirmationRequested(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneRFSetupConfirmed(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneRFSetupMismatch(Guid TimerId, byte Lane);
public record ImmersionRCLapRFTimerRFSetupConfirmationRequested(Guid TimerId);
public record ImmersionRCLapRFTimerRFSetupConfirmed(Guid TimerId);
public record ImmersionRCLapRFTimerRFSetupMismatch(Guid TimerId);
public record ImmersionRCLapRFLaneRFSetupSyncEnabled(Guid TimerId);
public record ImmersionRCLapRFLaneRFSetupSyncDisabled(Guid TimerId);
public record ImmersionRCLapRFTimerConnected(Guid TimerId);
public record ImmersionRCLapRFTimerDisconnected(Guid TimerId);