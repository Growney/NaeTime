﻿namespace NaeTime.OpenPractice.Messages.Events;
public record SingleLapLeaderboardRecordReduced(Guid SessionId, int NewPosition, int OldPosition, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);