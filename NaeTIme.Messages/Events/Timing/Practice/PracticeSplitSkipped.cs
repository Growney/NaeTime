namespace NaeTime.Messages.Events.Timing.Practice;
public record PracticeSplitSkipped(Guid SessionId, uint LapNumber, byte Split, byte Lane, Guid? PilotId);