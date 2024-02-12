namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class ConsecutiveLapsLeaderboardPosition
{
    public Guid Id { get; set; }
    public uint Position { get; set; }
    public Guid PilotId { get; set; }
    public string? PilotName { get; set; }
    public uint StartLapNumber { get; set; }
    public uint EndLapNumber { get; set; }
    public uint TotalLaps { get; set; }
    public long TotalMilliseconds { get; set; }
    public DateTime LastLapCompletionUtc { get; set; }
}

