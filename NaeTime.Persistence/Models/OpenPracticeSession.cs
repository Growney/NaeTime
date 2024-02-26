namespace NaeTime.Persistence.Models;
public partial record OpenPracticeSession(Guid SessionId, Guid TrackId, string? Name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<OpenPracticeLap> Laps,
    IEnumerable<PilotLane> ActiveLanes, IEnumerable<uint> TrackedConsecutiveLaps);