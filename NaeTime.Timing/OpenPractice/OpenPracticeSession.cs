using NaeTime.Timing.Models;
using NaeTime.Timing.OpenPractice.Leaderboards;

namespace NaeTime.Timing.Practice;
public class OpenPracticeSession
{

    public Guid SessionId { get; }
    private readonly Dictionary<Guid, List<Lap>> _pilotLaps = new();

    public OpenPracticeSession(Guid sessionId)
    {
        SessionId = sessionId;
    }

    public void AddCompletedLap(Guid pilotId, uint lapNumber, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, new List<Lap>());
        }

        _pilotLaps[pilotId].Add(new Lap(lapNumber, startedUtc, finishedUtc, LapStatus.Completed, totalMilliseconds));
    }
    public void AddInvalidLap(Guid pilotId, uint lapNumber, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, new List<Lap>());
        }

        _pilotLaps[pilotId].Add(new Lap(lapNumber, startedUtc, finishedUtc, LapStatus.Invalid, totalMilliseconds));
    }
    private IEnumerable<Lap> GetLaps(IEnumerable<Lap> laps, Lap? extraLap)
    {
        foreach (var lap in laps)
        {
            yield return lap;
        }
        if (extraLap != null)
        {
            yield return extraLap;
        }
    }
    private IEnumerable<KeyValuePair<Guid, IEnumerable<Lap>>> GetPilotLaps(Guid? extraLapPilotId, Lap? extraLap)
    {
        foreach (var pilotLap in _pilotLaps)
        {
            Lap? extraPilotLap = null;
            if (extraLapPilotId != null && pilotLap.Key == extraLapPilotId)
            {
                extraPilotLap = extraLap;
            }

            yield return new KeyValuePair<Guid, IEnumerable<Lap>>(pilotLap.Key, GetLaps(pilotLap.Value, extraPilotLap));
        }

    }
    private IEnumerable<SingleLapLeaderboardPosition> GetSingleLapLeaderboardPositions(IEnumerable<KeyValuePair<Guid, IEnumerable<Lap>>> pilotLaps)
    {
        var fastestSingleLaps = new List<(Guid pilotId, FastestSingleLap fastestLap)>();
        var singleLapCalculator = new FastestSingleLapCalculator();
        foreach (var pilot in pilotLaps)
        {
            var fastestLap = singleLapCalculator.Calculate(pilot.Value);
            if (fastestLap != null)
            {
                fastestSingleLaps.Add((pilot.Key, fastestLap));
            }
        }

        fastestSingleLaps.Sort(CompareSingleLaps);

        var leaderboard = new List<SingleLapLeaderboardPosition>();

        for (uint i = 0; i < fastestSingleLaps.Count; i++)
        {
            var fastestSingleLap = fastestSingleLaps[(int)i];
            leaderboard.Add(new SingleLapLeaderboardPosition(i, fastestSingleLap.pilotId, fastestSingleLap.fastestLap.LapNumber, fastestSingleLap.fastestLap.LapMilliseconds));
        }

        return leaderboard;
    }

    public IEnumerable<SingleLapLeaderboardPosition> GetSingleLapLeaderboardPositionsWithNewCompletedLap(Guid pilotId, uint lapNumber, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds)
    {
        return GetSingleLapLeaderboardPositions(GetPilotLaps(pilotId, new Lap(lapNumber, startedUtc, finishedUtc, LapStatus.Completed, totalMilliseconds)));
    }
    public IEnumerable<SingleLapLeaderboardPosition> GetSingleLapLeaderboardPositions()
    {
        return GetSingleLapLeaderboardPositions(GetPilotLaps(null, null));
    }
    private IEnumerable<ConsecutiveLapLeaderboardPosition> GetConsecutiveLapLeaderboardPositions(uint lapCount, IEnumerable<KeyValuePair<Guid, IEnumerable<Lap>>> pilotLaps)
    {
        var consecutiveLapCalculator = new FastestConsecutiveLapCalculator();
        var fastestConsecutiveLaps = new List<(Guid pilotId, FastestConsecutiveLaps lapCount)>();

        foreach (var pilot in pilotLaps)
        {
            var consecutiveLapCount = consecutiveLapCalculator.CalculateFastestConsecutiveLaps(lapCount, pilot.Value);
            fastestConsecutiveLaps.Add((pilot.Key, consecutiveLapCount));
        }
        fastestConsecutiveLaps.Sort(CompareConsecutiveLaps);

        var leaderboard = new List<ConsecutiveLapLeaderboardPosition>();
        for (uint positionIndex = 0; positionIndex < fastestConsecutiveLaps.Count; positionIndex++)
        {
            var fastestConsecutiveLap = fastestConsecutiveLaps[(int)positionIndex];
            leaderboard.Add(new ConsecutiveLapLeaderboardPosition(positionIndex, fastestConsecutiveLap.pilotId, fastestConsecutiveLap.lapCount.StartLapNumber, fastestConsecutiveLap.lapCount.EndLapNumber, fastestConsecutiveLap.lapCount.TotalLaps, fastestConsecutiveLap.lapCount.TotalMilliseconds));
        }

        return leaderboard;
    }
    public IEnumerable<ConsecutiveLapLeaderboardPosition> GetConsecutiveLapLeaderboardPositions(uint lapCount)
    {
        return GetConsecutiveLapLeaderboardPositions(lapCount, GetPilotLaps(null, null));
    }
    public IEnumerable<ConsecutiveLapLeaderboardPosition> GetConsecutiveLapLeaderboardPositionsWithNewCompletedLap(uint lapCount, Guid pilotId, uint lapNumber, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds)
    {
        return GetConsecutiveLapLeaderboardPositions(lapCount, GetPilotLaps(pilotId, new Lap(lapNumber, startedUtc, finishedUtc, LapStatus.Completed, totalMilliseconds)));
    }
    private int CompareSingleLaps((Guid pilotId, FastestSingleLap lapCount) x, (Guid pilotId, FastestSingleLap lapCount) y)
    {
        var timeComparison = x.lapCount.LapMilliseconds.CompareTo(y.lapCount.LapMilliseconds);
        if (timeComparison != 0)
        {
            return timeComparison;
        }
        return x.lapCount.CompletionUtc.CompareTo(y.lapCount.CompletionUtc);
    }
    private int CompareConsecutiveLaps((Guid pilotId, FastestConsecutiveLaps lapCount) x, (Guid pilotId, FastestConsecutiveLaps lapCount) y)
    {
        var lapCountComparison = x.lapCount.TotalLaps.CompareTo(y.lapCount.TotalLaps);
        if (lapCountComparison != 0)
        {
            return lapCountComparison;
        }
        var timeComparison = x.lapCount.TotalMilliseconds.CompareTo(y.lapCount.TotalMilliseconds);
        if (timeComparison != 0)
        {
            return timeComparison;
        }
        return x.lapCount.LastLapCompletionUtc.CompareTo(y.lapCount.LastLapCompletionUtc);
    }

}
