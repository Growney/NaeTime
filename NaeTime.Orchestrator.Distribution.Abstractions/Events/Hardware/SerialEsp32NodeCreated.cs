namespace NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;

public record SerialEsp32NodeCreated(Guid Id, string Name, string ComPort);