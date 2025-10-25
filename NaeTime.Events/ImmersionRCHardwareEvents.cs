namespace NaeTime.Events;

public record ImmersionRCLapRFNetworkDeviceRegistered(Guid TimerId, string Name, string IPAddress, ushort Port, byte Lanes);
public record ImmersionRCLapRFNetworkConfigurationChanged(Guid TimerId, string IPAddress, ushort Port);

public record ImmersionRCLapRFRenamed(Guid TimerId, string Name);

public record ImmersionRCLapRFLaneEnableRequested(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneDisableRequested(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneEnabled(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneDisabled(Guid TimerId, byte Lane);
public record ImmersionRCLapRFLaneStatusMismatch(Guid TimerId, byte Lane, bool DesiredStatus, bool ActualStatus);

public record ImmersionRCLapRFLaneFrequencyRequested(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record ImmersionRCLapRFLaneFrequencyTuned(Guid TimerId, byte Lane, byte? BandId, int FrequencyInMHz);
public record ImmersionRCLapRFLaneFrequencyMismatch(Guid TimerId, byte Lane, byte? DesiredBandId, int DesiredFrequencyInMHz, byte? ActualBandId, int ActualFrequencyInMHz);

public record ImmersionRCLapRFLaneThresholdRequested(Guid TimerId, byte Lane, float Threshold);
public record ImmersionRCLapRFLaneThresholdConfigured(Guid TimerId, byte Lane, float Threshold);
public record ImmersionRCLapRFLaneThresholdMismatch(Guid TimerId, byte Lane, float DesiredThreshold, float ActualThreshold);

public record ImmersionRCLapRFLaneGainRequested(Guid TimerId, byte Lane, ushort Gain);
public record ImmersionRCLapRFLaneGainConfigured(Guid TimerId, byte Lane, ushort Gain);
public record ImmersionRCLapRFLaneGainMismatch(Guid TimerId, byte Lane, ushort DesiredGain, ushort ActualGain);

public record ImmersionRCLapRFTimerConnected(Guid TimerId);
public record ImmersionRCLapRFTimerDisconnected(Guid TimerId);
public record ImmersionRCLapRFConfigurationUnconfirmed(Guid TimerId);