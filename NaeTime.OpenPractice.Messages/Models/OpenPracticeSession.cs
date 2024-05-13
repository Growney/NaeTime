﻿namespace NaeTime.OpenPractice.Messages.Models;
public record OpenPracticeSession(Guid Id, Guid TrackId, string? Name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Lap> Laps, IEnumerable<PilotLane> ActiveLanes, IEnumerable<uint> TrackedConsecutiveLaps);