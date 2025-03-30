namespace NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;

public record SerialEsp32NodeConfigured(Guid Id, string Name, string ComPort);
