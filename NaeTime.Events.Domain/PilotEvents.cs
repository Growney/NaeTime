namespace NaeTime.Events.Domain;

public record PilotBindingPhraseChanged(Guid PilotId, byte[] BindingPhrase);
public record PilotBindingPhraseRemoved(Guid PilotId);
public record PilotCallsignAssigned(Guid PilotId, string? CallSign);
public record PilotRegistered(Guid PilotId);
public record PilotRenamed(Guid PilotId, string? Firstname, string? Lastname);

public record PilotImmersionRCTimerGainConfigured(Guid PilotId, Guid TimerId, ushort Gain);
public record PilotImmersionRCGainConfigured(Guid PilotId, ushort Gain);
public record PilotImmersionRCTimerGainReset(Guid PilotId, Guid TimerId);
public record PilotImmersionRCGainReset(Guid PilotId);

public record PilotImmersionRCThresholdConfigured(Guid PilotId, float Threshold);
public record PilotImmersionRCDetectorThresholdConfigured(Guid PilotId, Guid DetectorId, float Threshold);
public record PilotImmersionRCThresholdReset(Guid PilotId);
public record PilotImmersionRCDetectorThresholdReset(Guid PilotId, Guid DetectorId);

public record PilotNaeTimeNodeEntryThresholdConfigured(Guid PilotId, ushort Threshold);
public record PilotNaeTimeNodeDetectorEntryThresholdConfigured(Guid PilotId, Guid DetectorId, ushort Threshold);
public record PilotNaeTimeNodeEntryThresholdReset(Guid PilotId);
public record PilotNaeTimeNodeDetectorEntryThresholdReset(Guid PilotId, Guid DetectorId);

public record PilotNaeTimeNodeExitThresholdConfigured(Guid PilotId, ushort Threshold);
public record PilotNaeTimeNodeDetectorExitThresholdConfigured(Guid PilotId, Guid DetectorId, ushort Threshold);
public record PilotNaeTimeNodeExitThresholdReset(Guid PilotId);
public record PilotNaeTimeNodeDetectorExitThresholdReset(Guid PilotId, Guid DetectorId);