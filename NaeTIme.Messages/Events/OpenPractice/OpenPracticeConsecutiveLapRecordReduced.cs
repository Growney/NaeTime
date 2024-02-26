﻿namespace NaeTime.Messages.Events.OpenPractice;
public record OpenPracticeConsecutiveLapRecordReduced(Guid SessionId, Guid PilotId, uint LapCap, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
