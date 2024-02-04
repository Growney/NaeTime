namespace NaeTime.Messages.Events.Entities;
public record PilotDetailsChanged(Guid PilotId, string FirstName, string LastName, string CallSign);