﻿namespace NaeTime.Persistence.SQLite.Models;
public class OpenPracticeLap
{
    public Guid Id { get; set; }
    public Guid PilotId { get; set; }
    public uint LapNumber { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime FinishedUtc { get; set; }
    public OpenPracticeLapStatus Status { get; set; }
    public long TotalMilliseconds { get; set; }
}