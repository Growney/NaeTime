namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeLapRemoved(Guid SessionId, Guid LapId, Guid PilotId);
