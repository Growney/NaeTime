namespace NaeTime.Management.Messages;
public record PilotDetailsChanged(Guid PilotId, string? FirstName, string? LastName, string? CallSign);