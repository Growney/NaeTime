using EventDbLite.Projections;
using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class OpenPracticeTiming : Projection
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ConcurrentBag<OpenPracticeDetection>>>> _sessionTrackPilotDetections = new();

    private ConcurrentDictionary<Guid, ConcurrentBag<OpenPracticeDetection>> GetSessionDetections(Guid sessionId, Guid trackId)
    {
        ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ConcurrentBag<OpenPracticeDetection>>> sessionRecords = _sessionTrackPilotDetections.GetOrAdd(sessionId, _ => new());
        return sessionRecords.GetOrAdd(trackId, _ => new());
    }
    private ConcurrentBag<OpenPracticeDetection> GetPilotDetections(Guid sessionId, Guid trackId, Guid pilotId)
    {
        ConcurrentDictionary<Guid, ConcurrentBag<OpenPracticeDetection>> trackRecords = GetSessionDetections(sessionId, trackId);
        return trackRecords.GetOrAdd(pilotId, _ => new());
    }

    public void When(OpenPracticePilotDetectionTriggered triggered)
    {
        ConcurrentBag<OpenPracticeDetection> pilotDetections = GetPilotDetections(triggered.SessionId, triggered.TrackId, triggered.PilotId);

        pilotDetections.Add(new OpenPracticeDetection(triggered.DetectionId, triggered.SessionId, triggered.PilotId, null, true, triggered.OrdinalPosition, triggered.TrackDetectorCount, triggered.Lane, triggered.HardwareTime, triggered.SoftwareTime, triggered.UtcTime));
    }
    public void When(OpenPracticePilotDetectionOccured occured)
    {
        ConcurrentBag<OpenPracticeDetection> pilotDetections = GetPilotDetections(occured.SessionId, occured.TrackId, occured.PilotId);

        pilotDetections.Add(new OpenPracticeDetection(occured.DetectionId, occured.SessionId, occured.PilotId, occured.TimerId, true, occured.TrackTimerOrdinal, occured.TrackTimerTotal, occured.Lane, occured.HardwareTime, occured.SoftwareTime, occured.UtcTime));
    }

    private static TimeSpan CalculateDuration(Guid? startTimerId, ulong? startHardwareTime, long startSoftwareTime, DateTime startUtcTime, Guid? endTimerId, ulong? endHardwareTime, long endSoftwareTime, DateTime endUtcTime)
    {
        if (startTimerId.HasValue && endTimerId.HasValue && startTimerId == endTimerId && startHardwareTime.HasValue && endHardwareTime.HasValue)
        {
            if (endHardwareTime.Value >= startHardwareTime.Value)
            {
                return TimeSpan.FromMilliseconds(endHardwareTime.Value - startHardwareTime.Value);
            }
        }
        if (endSoftwareTime >= startSoftwareTime)
        {
            return TimeSpan.FromMilliseconds(endSoftwareTime - startSoftwareTime);
        }
        if (endUtcTime >= startUtcTime)
        {
            return endUtcTime - startUtcTime;
        }
        return CalculateDuration(endTimerId, endHardwareTime, endSoftwareTime, endUtcTime, startTimerId, startHardwareTime, startSoftwareTime, startUtcTime);
    }
    private static IEnumerable<OpenPracticeLap> GetPilotSessionLaps(Guid sessionId, Guid trackId, Guid pilotId, IEnumerable<OpenPracticeDetection> pilotDetections, TimeSpan minimumLapTime, TimeSpan maximumLapTime)
    {
        if (!pilotDetections.Any())
        {
            return Enumerable.Empty<OpenPracticeLap>();
        }

        List<OpenPracticeDetection> orderedDetections = pilotDetections.OrderBy(d => d.UtcTime).ToList();

        int firstValidDetectionIndex = orderedDetections.FindIndex(d => d.IsValid && d.TrackTimerOrdinal == 0);

        if (firstValidDetectionIndex < 0)
        {
            return Enumerable.Empty<OpenPracticeLap>();
        }

        List<OpenPracticeLap> laps = new();

        OpenPracticeDetection initialDetection = orderedDetections[firstValidDetectionIndex];
        for (int i = firstValidDetectionIndex + 1; i < orderedDetections.Count - 1; i++)
        {
            OpenPracticeDetection currentDetection = orderedDetections[i];

            //We shall skip split times for now
            if (currentDetection.TrackTimerOrdinal != 0)
            {
                continue;
            }

            if (!currentDetection.IsValid)
            {
                continue;
            }

            TimeSpan duration = CalculateDuration(
                initialDetection.TimerId, initialDetection.HardwareTime, initialDetection.SoftwareTime, initialDetection.UtcTime,
                currentDetection.TimerId, currentDetection.HardwareTime, currentDetection.SoftwareTime, currentDetection.UtcTime);

            if (duration < minimumLapTime)
            {
                continue;
            }

            if (duration > maximumLapTime)
            {
                initialDetection = currentDetection;
                continue;
            }

            OpenPracticeLap lap = new(
                SessionId: sessionId,
                TrackId: trackId,
                PilotId: pilotId,
                StartDetection: initialDetection,
                EndDetection: currentDetection,
                Duration: duration);

            laps.Add(lap);

            initialDetection = currentDetection;
        }

        return laps;
    }
    private static List<List<OpenPracticeLap>> GetConsecutiveLapGroups(IEnumerable<OpenPracticeLap> laps)
    {
        List<OpenPracticeLap> orderedLaps = laps.OrderBy(l => l.StartDetection.UtcTime).ToList();

        List<List<OpenPracticeLap>> lapGroups = new();

        List<OpenPracticeLap> currentGroup = new();

        for (int i = 0; i < orderedLaps.Count; i++)
        {
            // Always add the first lap to start a new group
            if (i == 0)
            {
                currentGroup.Add(orderedLaps[i]);
                continue;
            }

            OpenPracticeLap previousLap = orderedLaps[i - 1];
            OpenPracticeLap currentLap = orderedLaps[i];

            if (previousLap.EndDetection.Id != currentLap.StartDetection.Id)
            {
                // Not consecutive, start a new group
                lapGroups.Add(currentGroup);
                currentGroup = new List<OpenPracticeLap> { currentLap };
            }
            else
            {
                // Consecutive, add to the current group
                currentGroup.Add(currentLap);
            }
        }

        if (currentGroup.Any())
        {
            lapGroups.Add(currentGroup);
        }

        return lapGroups;
    }
    private static Dictionary<uint, OpenPracticeLapRecord> GetGroupRecords(Guid sessionId, Guid trackId, Guid pilotId, List<OpenPracticeLap> lapGroup)
    {
        Dictionary<uint, OpenPracticeLapRecord> records = new();
        for (uint groupSize = 1; groupSize <= lapGroup.Count; groupSize++)
        {
            TimeSpan? bestDuration = null;
            List<OpenPracticeLap> bestLaps = new();

            // Slide a window of size groupSize over the validLaps
            for (int start = 0; start <= lapGroup.Count - groupSize; start++)
            {
                IEnumerable<OpenPracticeLap> group = lapGroup.Skip(start).Take((int)groupSize);
                TimeSpan totalDuration = group.Aggregate(TimeSpan.Zero, (sum, lap) => sum + lap.Duration);

                if (bestDuration == null || totalDuration < bestDuration)
                {
                    bestDuration = totalDuration;
                    bestLaps.Clear();
                    bestLaps.AddRange(group);
                }
            }

            if (bestDuration != null)
            {
                records[groupSize] = new OpenPracticeLapRecord(sessionId, trackId, pilotId, groupSize, bestLaps, bestDuration.Value);
            }
        }

        return records;
    }
    private static Dictionary<uint, OpenPracticeLapRecord> GetPilotRecords(Guid sessionId, Guid trackId, Guid pilotId, IEnumerable<OpenPracticeLap> laps)
    {
        List<List<OpenPracticeLap>> lapGroups = GetConsecutiveLapGroups(laps);
        Dictionary<uint, OpenPracticeLapRecord> pilotRecords = new();
        foreach (List<OpenPracticeLap> lapGroup in lapGroups)
        {
            Dictionary<uint, OpenPracticeLapRecord> groupRecords = GetGroupRecords(sessionId, trackId, pilotId, lapGroup);
            foreach (var record in groupRecords)
            {
                if (!pilotRecords.ContainsKey(record.Key) || record.Value.Record < pilotRecords[record.Key].Record)
                {
                    pilotRecords[record.Key] = record.Value;
                }
            }
        }
        return pilotRecords;
    }
    private static Dictionary<uint, IEnumerable<OpenPracticeLapRecord>> GetSessionRecords(Dictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> pilotRecords)
    {
        Dictionary<uint, List<OpenPracticeLapRecord>> sessionRecords = new();
        foreach (var pilotEntry in pilotRecords)
        {
            foreach (var recordEntry in pilotEntry.Value)
            {
                uint lapCount = recordEntry.Key;
                OpenPracticeLapRecord record = recordEntry.Value;
                if (!sessionRecords.ContainsKey(lapCount))
                {
                    sessionRecords[lapCount] = new List<OpenPracticeLapRecord>();
                }
                sessionRecords[lapCount].Add(record);
            }
        }
        return sessionRecords.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<OpenPracticeLapRecord>)kvp.Value);
    }

    public OpenPracticeSessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime)
    {
        ConcurrentDictionary<Guid, ConcurrentBag<OpenPracticeDetection>> trackPilotsDetections = GetSessionDetections(sessionId, trackId);

        Dictionary<Guid, IEnumerable<OpenPracticeDetection>> allDetections = new();
        Dictionary<Guid, IEnumerable<OpenPracticeLap>> allLaps = new();
        Dictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> allRecords = new();

        foreach (KeyValuePair<Guid, ConcurrentBag<OpenPracticeDetection>> pilotEntry in trackPilotsDetections)
        {
            Guid pilotId = pilotEntry.Key;

            IEnumerable<OpenPracticeDetection> pilotDetections = pilotEntry.Value.ToList();
            allDetections[pilotId] = pilotDetections;
            IEnumerable<OpenPracticeLap> pilotLaps = GetPilotSessionLaps(sessionId, trackId, pilotId, pilotDetections, minimumLapTime, maximumLapTime);
            allLaps[pilotId] = pilotLaps;
            allRecords[pilotId] = GetPilotRecords(sessionId, trackId, pilotId, pilotLaps);
        }

        Dictionary<uint, IEnumerable<OpenPracticeLapRecord>> sessionRecords = GetSessionRecords(allRecords);

        return new OpenPracticeSessionTimingInformation(allDetections, allLaps, allRecords, sessionRecords);
    }
    public OpenPracticeSessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId)
    {
        ConcurrentBag<OpenPracticeDetection> detections = GetPilotDetections(sessionId, trackId, pilotId);

        IEnumerable<OpenPracticeDetection> pilotDetections = detections.ToList();
        IEnumerable<OpenPracticeLap> pilotLaps = GetPilotSessionLaps(sessionId, trackId, pilotId, pilotDetections, TimeSpan.Zero, TimeSpan.MaxValue);
        IDictionary<uint, OpenPracticeLapRecord> pilotRecords = GetPilotRecords(sessionId, trackId, pilotId, pilotLaps);

        return new OpenPracticeSessionPilotTimingInfo(pilotDetections, pilotLaps, pilotRecords);
    }
}
