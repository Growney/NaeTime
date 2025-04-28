using static NaeTime.Persistence.Abstractions.Management.ActiveSession;

namespace NaeTime.Orchestrator.Distribution.Abstractions.Events.Management;
public record SessionActivated(Guid SessionId, SessionType Type);
