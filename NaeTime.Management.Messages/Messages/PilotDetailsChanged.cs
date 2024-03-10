namespace NaeTime.Management.Messages.Messages;
public record PilotDetailsChanged(Guid PilotId, string? FirstName, string? LastName, string? CallSign);