using NaeTime.Timing.Models;

namespace NaeTime.Timing;
public class FastestConsecutiveLapCalculator
{
    public FastestConsecutiveLaps CalculateFastestConsecutiveLaps(uint lapCount, IEnumerable<Lap> laps)
    {
        var lapsList = laps.ToList();

        long totalTime = 0;
        uint startLapNumber = 0;
        uint endLapNumber = 0;

        long minTotalTime = long.MaxValue;
        uint maxConsecutiveLaps = 0;

        DateTime lastLapCompletion = DateTime.MinValue;

        for (uint currentLapIndex = 0; currentLapIndex < lapsList.Count; currentLapIndex++)
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
            uint currentConsecutiveLaps = 1;

            if (currentConsecutiveLaps >= maxConsecutiveLaps)
            {
                if (totalTime < minTotalTime)
                {
                    maxConsecutiveLaps = currentConsecutiveLaps;
                    minTotalTime = totalTime;
                    startLapNumber = startLap.LapNumber;
                    endLapNumber = startLap.LapNumber;
                    lastLapCompletion = startLap.FinishedUtc;
                }
            }


            for (uint currentCheckingLapIndex = currentLapIndex + 1; currentCheckingLapIndex < lapsList.Count; currentCheckingLapIndex++)
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
                            startLapNumber = startLap.LapNumber;
                            endLapNumber = currentCheckingLap.LapNumber;
                            lastLapCompletion = currentCheckingLap.FinishedUtc;
                        }
                    }
                    else if (currentConsecutiveLaps > maxConsecutiveLaps)
                    {
                        maxConsecutiveLaps = currentConsecutiveLaps;
                        minTotalTime = totalTime;
                        startLapNumber = startLap.LapNumber;
                        endLapNumber = currentCheckingLap.LapNumber;
                        lastLapCompletion = currentCheckingLap.FinishedUtc;
                    }
                }
            }
        }
        return new FastestConsecutiveLaps(startLapNumber, endLapNumber, maxConsecutiveLaps, minTotalTime, lastLapCompletion);
    }
}
