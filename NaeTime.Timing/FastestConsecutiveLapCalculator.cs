using NaeTime.Timing.Models;

namespace NaeTime.Timing;
public class FastestConsecutiveLapCalculator
{
    public FastestConsecutiveLaps? CalculateFastestConsecutiveLaps(uint lapCount, IEnumerable<Lap> laps)
    {
        if (!laps.Any(x => x.Status == LapStatus.Completed))
        {
            return null;
        }

        var lapsList = laps.ToList();

        lapsList.Sort((x, y) => x.FinishedUtc.CompareTo(y.FinishedUtc));

        long totalTime = 0;
        int startLapNumber = 0;
        int endLapNumber = 0;

        long minTotalTime = long.MaxValue;
        int maxConsecutiveLaps = 0;
        var lapQueue = new Queue<Guid>();
        DateTime lastLapCompletion = DateTime.MinValue;

        for (int currentLapIndex = 0; currentLapIndex < lapsList.Count; currentLapIndex++)
        {
            var startLap = lapsList[(int)currentLapIndex];
            totalTime = startLap.TotalMilliseconds;
            //skip the checking of not completed laps
            if (startLap.Status != LapStatus.Completed)
            {
                continue;
            }
            var remainingLaps = lapsList.Count - currentLapIndex;

            //We have less remaining laps than the lap count and less than our max consecutive laps so we know we aren't gonna find a new record
            if (remainingLaps < lapCount && remainingLaps < maxConsecutiveLaps)
            {
                break;
            }
            int currentConsecutiveLaps = 1;

            if (currentConsecutiveLaps >= maxConsecutiveLaps)
            {
                if (totalTime < minTotalTime)
                {
                    maxConsecutiveLaps = currentConsecutiveLaps;
                    minTotalTime = totalTime;
                    startLapNumber = currentLapIndex;
                    endLapNumber = currentLapIndex;
                    lastLapCompletion = startLap.FinishedUtc;
                }
            }


            for (int currentCheckingLapIndex = currentLapIndex + 1; currentCheckingLapIndex < lapsList.Count; currentCheckingLapIndex++)
            {
                currentConsecutiveLaps = (currentCheckingLapIndex - currentLapIndex) + 1;

                //The laps are too separate so we can break
                if (currentConsecutiveLaps > lapCount)
                {
                    break;
                }

                var currentCheckingLap = lapsList[(int)currentCheckingLapIndex];
                if (currentCheckingLap.Status != LapStatus.Completed)
                {
                    break;
                }

                //Continuation of laps
                if (currentCheckingLap.Status == LapStatus.Completed)
                {
                    totalTime += currentCheckingLap.TotalMilliseconds;
                    if (currentConsecutiveLaps == maxConsecutiveLaps)
                    {
                        if (totalTime < minTotalTime)
                        {
                            minTotalTime = totalTime;
                            startLapNumber = currentLapIndex;
                            endLapNumber = currentLapIndex;
                            lastLapCompletion = currentCheckingLap.FinishedUtc;
                        }
                    }
                    else if (currentConsecutiveLaps > maxConsecutiveLaps)
                    {
                        maxConsecutiveLaps = currentConsecutiveLaps;
                        minTotalTime = totalTime;
                        startLapNumber = currentLapIndex;
                        endLapNumber = currentCheckingLapIndex;
                        lastLapCompletion = currentCheckingLap.FinishedUtc;
                    }
                }
            }
        }

        return new FastestConsecutiveLaps((uint)maxConsecutiveLaps, minTotalTime, lastLapCompletion, laps.Skip(startLapNumber).Take(maxConsecutiveLaps).Select(x => x.LapId));
    }
}
