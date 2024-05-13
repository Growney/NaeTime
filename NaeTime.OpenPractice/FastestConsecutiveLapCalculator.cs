using NaeTime.OpenPractice.Leaderboards;
using NaeTime.OpenPractice.Models;

namespace NaeTime.OpenPractice;
public class FastestConsecutiveLapCalculator
{
    public ConsecutiveLapRecord? Calculate(uint lapCount, IEnumerable<Lap> laps)
    {
        if (!laps.Any(x => x.Status == LapStatus.Completed))
        {
            return null;
        }

        List<Lap> lapsList = laps.ToList();

        long totalTime = 0;
        int startLapNumber = 0;
        int endLapNumber = 0;

        long minTotalTime = long.MaxValue;
        int maxConsecutiveLaps = 0;
        Queue<Guid> lapQueue = new();
        DateTime lastLapCompletion = DateTime.MinValue;

        for (int currentLapIndex = 0; currentLapIndex < lapsList.Count; currentLapIndex++)
        {
            Lap startLap = lapsList[currentLapIndex];
            totalTime = startLap.TotalMilliseconds;
            //skip the checking of not completed laps
            if (startLap.Status != LapStatus.Completed)
            {
                continue;
            }

            int remainingLaps = lapsList.Count - currentLapIndex;

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
                currentConsecutiveLaps = currentCheckingLapIndex - currentLapIndex + 1;

                //The laps are too separate so we can break
                if (currentConsecutiveLaps > lapCount)
                {
                    break;
                }

                Lap currentCheckingLap = lapsList[currentCheckingLapIndex];
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

        return new ConsecutiveLapRecord((uint)maxConsecutiveLaps, minTotalTime, lastLapCompletion, laps.Skip(startLapNumber).Take(maxConsecutiveLaps).Select(x => x.LapId));
    }
}
