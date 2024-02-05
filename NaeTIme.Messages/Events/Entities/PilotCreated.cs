namespace NaeTime.Messages.Events.Entities;
public record PilotCreated(Guid PilotId, string? FirstName, string? LastName, string? CallSign);