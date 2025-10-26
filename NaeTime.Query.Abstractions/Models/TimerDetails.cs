namespace NaeTime.Query.Abstractions.Models;
public record TimerDetails(Guid Id, string Name,bool IsConnected, IEnumerable<TimerLaneDetails> Lanes);