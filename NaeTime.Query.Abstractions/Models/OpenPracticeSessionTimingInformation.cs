namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionTimingInformation(
    IDictionary<Guid, IEnumerable<OpenPracticeTimingMoment>> Moments,
    IDictionary<Guid, IDictionary<Guid, OpenPracticeDetection>> PilotDetections,
    IDictionary<Guid, IEnumerable<IEnumerable<OpenPracticeLap>>> PilotLapGroups,
    IDictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> PilotLapRecords,
    IDictionary<uint, IEnumerable<OpenPracticeLapRecord>> SessionLapRecords)
{
    public TimeSpan GetAverageLapDuration(Guid pilotId)
    {
        if (!PilotLapGroups.TryGetValue(pilotId, out var lapGroups) || !lapGroups.Any())
        {
            return TimeSpan.Zero;
        }

        IEnumerable<TimeSpan> lapDurations = lapGroups.SelectMany(group => group.Select(lap => lap.Duration));

        double averageTicks = lapDurations.Average(duration => duration.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(averageTicks);
    }
    public int GetLapCount(Guid pilotId)
    {
        if (!PilotLapGroups.TryGetValue(pilotId, out var lapGroups) || !lapGroups.Any())
        {
            return 0;
        }
        return lapGroups.SelectMany(group => group.Select(lap => lap)).Count();
    }
    public TimeSpan? GetLapStandardDeviation(Guid pilotId, float topPercentage = 100f)
    {
        if (!PilotLapGroups.TryGetValue(pilotId, out var lapGroups) || !lapGroups.Any())
        {
            return null;
        }
        IEnumerable<double> lapDurations = lapGroups.SelectMany(group => group.Select(lap => lap.Duration.TotalMilliseconds));
        int countToTake = (int)(lapDurations.Count() * topPercentage);
        var topLapDurations = lapDurations.OrderBy(d => d).Take(countToTake);

        if (!topLapDurations.Any())
        {
            return null;
        }

        double average = topLapDurations.Average();
        double sumOfSquaresOfDifferences = topLapDurations.Select(val => (val - average) * (val - average)).Sum();
        double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / topLapDurations.Count());
        return TimeSpan.FromMilliseconds(standardDeviation);
    }
    public TimeSpan GetLapRecord(Guid pilotId, uint lapNumber)
    {
        if (!PilotLapRecords.TryGetValue(pilotId, out var lapRecords) || !lapRecords.TryGetValue(lapNumber, out var lapRecord))
        {
            return TimeSpan.Zero;
        }
        return lapRecord.Record;
    }
    public int GetLapRecordLeaderboardPosition(Guid pilotId, uint lapNumber)
    {
        if (!PilotLapRecords.TryGetValue(pilotId, out var lapRecords) || !lapRecords.TryGetValue(lapNumber, out var lapRecord))
        {
            return -1;
        }
        if (!SessionLapRecords.TryGetValue(lapNumber, out var sessionLapRecords))
        {
            return -1;
        }
        var orderedRecords = sessionLapRecords.OrderBy(record => record.Record).ToList();
        int position = orderedRecords.FindIndex(record => record.PilotId == pilotId) + 1;
        return position;
    }
    public TimeSpan GetLastLapDuration(Guid pilotId)
    {
        if (!PilotLapGroups.TryGetValue(pilotId, out var lapGroups) || !lapGroups.Any())
        {
            return TimeSpan.Zero;
        }
        var lastLap = lapGroups.SelectMany(group => group.Select(lap => lap)).OrderBy(l => l.Duration).LastOrDefault();
        if (lastLap == null)
        {
            return TimeSpan.Zero;
        }
        return lastLap.Duration;
    }
    public DateTime? GetLastDetection(Guid pilotId)
    {
        if (!PilotDetections.TryGetValue(pilotId, out var detections) || !detections.Any())
        {
            return null;
        }
        var lastDetection = detections.Values.OrderBy(d => d.UtcTime).LastOrDefault();
        if (lastDetection == null)
        {
            return null;
        }
        return lastDetection.UtcTime;
    }
}