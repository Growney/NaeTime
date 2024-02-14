namespace NaeTime.Client.Razor.Lib.Models.OpenPractice;
public class SingleLapLeaderboardPosition
{
    public Guid Id { get; set; }
    public uint Position { get; set; }
    public Guid PilotId { get; set; }
    public string? PilotName { get; set; }
    public Guid LapId { get; set; }
    public long LapMilliseconds { get; set; }
    public DateTime CompletionUtc { get; set; }
}

