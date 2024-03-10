namespace NaeTime.Management.Messages.Messages;
public record PilotCreated(Guid PilotId, string? FirstName, string? LastName, string? CallSign);