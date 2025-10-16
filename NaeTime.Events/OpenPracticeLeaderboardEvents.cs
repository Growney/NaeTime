namespace NaeTime.Events;

public record PilotTookSessionConsecutiveLapsPolePosition(Guid PilotId, Guid SessionId, Guid TrackId, uint LapCount, Guid[] IncludedLaps, TimeSpan Record);
public record PilotTookTrackConsecutiveLapsPolePosition(Guid PilotId, Guid SessionId, Guid TrackId, uint LapCount, Guid[] IncludedLaps, TimeSpan Record);
public record PilotTookSessionFastestLapPolePosition(Guid PilotId, Guid SessionId, Guid TrackId, TimeSpan Record, Guid LapId);
public record PilotTookTrackFastestLapPolePosition(Guid PilotId, Guid SessionId, Guid TrackId, TimeSpan Record, Guid LapId);
public record PilotAchievedNewSessionConsecutiveLapPersonalBest(Guid PilotId, Guid SessionId, Guid TrackId, uint LapCount, Guid[] IncludedLaps, TimeSpan Record);
public record PilotAchievedNewSessionFastestLapPersonalBest(Guid PilotId, Guid SessionId, Guid TrackId, TimeSpan Record, Guid LapId);
public record PilotAchievedNewTrackConsecutiveLapPersonalBest(Guid PilotId, Guid SessionId, Guid TrackId, uint LapCount, Guid[] IncludedLaps, TimeSpan Record);
public record PilotAchievedNewTrackFastestLapPersonalBest(Guid PilotId, Guid SessionId, Guid TrackId, TimeSpan Record, Guid LapId);
public record PilotCompletedLapThatHadNoAffectOnLeaderboard(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, TimeSpan Duration);