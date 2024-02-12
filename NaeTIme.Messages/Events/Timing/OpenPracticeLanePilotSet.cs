namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeLanePilotSet(Guid SessionId, Guid PilotId, byte Lane);
