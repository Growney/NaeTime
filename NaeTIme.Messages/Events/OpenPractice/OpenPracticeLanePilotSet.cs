namespace NaeTime.Messages.Events.OpenPractice;
public record OpenPracticeLanePilotSet(Guid SessionId, Guid PilotId, byte Lane);
