namespace NaeTime.Hardware.Messages.Models;
public record TimerDetails(Guid Id, string? Name, TimerType Type, byte MaxLanes);