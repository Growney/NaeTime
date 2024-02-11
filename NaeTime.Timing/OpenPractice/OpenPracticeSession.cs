using NaeTime.Timing.Models;
using NaeTime.Timing.OpenPractice;

namespace NaeTime.Timing.Practice;
public class OpenPracticeSession
{

    public Guid SessionId { get; }
    private readonly Dictionary<Guid, List<Lap>> _pilotLaps = new Dictionary<Guid, List<Lap>>();

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
    public void AddTooLongLap(Guid pilotId, uint lapNumber, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, new List<Lap>());
        }

        _pilotLaps[pilotId].Add(new Lap(lapNumber, startedUtc, finishedUtc, LapStatus.TooLong, totalMilliseconds));
    }
    public void AddTooShortLap(Guid pilotId, uint lapNumber, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds)
    {
        if (!_pilotLaps.ContainsKey(pilotId))
        {
            _pilotLaps.Add(pilotId, new List<Lap>());
        }

        _pilotLaps[pilotId].Add(new Lap(lapNumber, startedUtc, finishedUtc, LapStatus.TooShort, totalMilliseconds));
    }

    public IEnumerable<OpenPracticeSingleLapLeaderboardPosition> GetSingleLapLeaderboardPositions()
    {
        var fastestSingleLaps = new List<(Guid pilotId, FastestSingleLap fastestLap)>();
        var singleLapCalculator = new FastestSingleLapCalculator();
        foreach (var pilot in _pilotLaps)
        {
            var fastestLap = singleLapCalculator.Calculate(pilot.Value);
            if (fastestLap != null)
            {
                fastestSingleLaps.Add((pilot.Key, fastestLap));
            }
        }

        fastestSingleLaps.Sort(CompareSingleLaps);

        var leaderboard = new List<OpenPracticeSingleLapLeaderboardPosition>();

        for (uint i = 0; i < fastestSingleLaps.Count; i++)
        {
            var fastestSingleLap = fastestSingleLaps[(int)i];
            leaderboard.Add(new OpenPracticeSingleLapLeaderboardPosition(i, fastestSingleLap.pilotId, fastestSingleLap.fastestLap.LapNumber, fastestSingleLap.fastestLap.LapMilliseconds));
        }

        return leaderboard;
    }
    public IEnumerable<OpenPracticeConsecutiveLapLeaderboardPosition> GetConsecutiveLapLeaderboardPositions(uint lapCount)
    {
        var consecutiveLapCalculator = new FastestConsecutiveLapCalculator();
        var fastestConsecutiveLaps = new List<(Guid pilotId, FastestConsecutiveLaps lapCount)>();

        foreach (var pilot in _pilotLaps)
        {
            var consecutiveLapCount = consecutiveLapCalculator.CalculateFastestConsecutiveLaps(lapCount, pilot.Value);
            fastestConsecutiveLaps.Add((pilot.Key, consecutiveLapCount));
        }
        fastestConsecutiveLaps.Sort(CompareConsecutiveLaps);

        var leaderboard = new List<OpenPracticeConsecutiveLapLeaderboardPosition>();
        for (uint positionIndex = 0; positionIndex < fastestConsecutiveLaps.Count; positionIndex++)
        {
            var fastestConsecutiveLap = fastestConsecutiveLaps[(int)positionIndex];
            leaderboard.Add(new OpenPracticeConsecutiveLapLeaderboardPosition(positionIndex, fastestConsecutiveLap.pilotId, fastestConsecutiveLap.lapCount.StartLapNumber, fastestConsecutiveLap.lapCount.EndLapNumber, fastestConsecutiveLap.lapCount.TotalLaps, fastestConsecutiveLap.lapCount.TotalMilliseconds));
        }

        return leaderboard;
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
