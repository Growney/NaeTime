namespace NaeTime.Query.Abstractions.Models;
public record TimerLaneDetails(Guid Id, string Name, byte LaneId, TimerStatus Status);