﻿namespace NaeTime.OpenPractice.SQLite.Models;
internal class ConsecutiveLapLeaderboardPosition
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public Guid SessionId { get; set; }
    public Guid PilotId { get; set; }
    public uint LapCap { get; set; }
    public uint TotalLaps { get; set; }
    public long TotalMilliseconds { get; set; }
    public DateTime LastLapCompletionUtc { get; set; }
    public List<IncludedLap> IncludedLaps { get; set; } = new List<IncludedLap>();
}
