namespace NaeTime.OpenPractice.Messages.Events;
public record OpenPracticeLapRemoved(Guid SessionId, Guid LapId, Guid PilotId);
