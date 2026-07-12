namespace NaeTime.Query.Abstractions.Models;
public record SessionPilotTimingInfo(
    IEnumerable<TimingMoment> Moments,
    IDictionary<Guid, Detection> IndexedDetections,
    IEnumerable<IEnumerable<OpenPracticeLap>> LapGroups,
    IDictionary<uint, OpenPracticeLapRecord> LapRecords)
{
    public IEnumerable<Detection> Detections => IndexedDetections.Values;

    public TimeSpan GetAverageLapDuration()
    {
        if (!LapGroups.Any())
        {
            return TimeSpan.Zero;
        }

        IEnumerable<TimeSpan> lapDurations = LapGroups.SelectMany(group => group.Select(lap => lap.Duration));

        double averageTicks = lapDurations.Average(duration => duration.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(averageTicks);
    }
    public int GetLapCount()
    {
        if (!LapGroups.Any())
        {
            return 0;
        }
        return LapGroups.SelectMany(group => group.Select(lap => lap)).Count();
    }
    public float GetLapStandardDeviation()
    {
        if (!LapGroups.Any())
        {
            return 0f;
        }
        IEnumerable<double> lapDurations = LapGroups.SelectMany(group => group.Select(lap => lap.Duration.TotalMilliseconds));
        double average = lapDurations.Average();
        double sumOfSquaresOfDifferences = lapDurations.Select(val => (val - average) * (val - average)).Sum();
        double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / lapDurations.Count());
        return (float)standardDeviation;
    }

    public float GetLapStandardDeviation(float topPercentage)
    {
        if (!LapGroups.Any())
        {
            return 0f;
        }
        IEnumerable<double> lapDurations = LapGroups.SelectMany(group => group.Select(lap => lap.Duration.TotalMilliseconds));
        int countToTake = (int)(lapDurations.Count() * topPercentage);
        var topLapDurations = lapDurations.OrderByDescending(d => d).Take(countToTake);
        if (!topLapDurations.Any())
        {
            return 0f;
        }
        double average = topLapDurations.Average();
        double sumOfSquaresOfDifferences = topLapDurations.Select(val => (val - average) * (val - average)).Sum();
        double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / topLapDurations.Count());
        return (float)standardDeviation;
    }
}
