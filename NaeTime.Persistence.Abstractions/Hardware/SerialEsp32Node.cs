namespace NaeTime.Persistence.Abstractions.Hardware;
public record class SerialEsp32Node(Guid TimerId, string Name, string Port);