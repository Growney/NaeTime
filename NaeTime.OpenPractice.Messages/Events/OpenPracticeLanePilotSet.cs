namespace NaeTime.OpenPractice.Messages.Events;
public record OpenPracticeLanePilotSet(Guid SessionId, Guid PilotId, byte Lane);
