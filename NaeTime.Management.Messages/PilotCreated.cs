namespace NaeTime.Management.Messages;
public record PilotCreated(Guid PilotId, string? FirstName, string? LastName, string? CallSign);