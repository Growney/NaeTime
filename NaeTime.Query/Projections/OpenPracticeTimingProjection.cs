using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class OpenPracticeTimingProjection : IOpenPracticeTimingProjection
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

    private void When(OpenPracticePilotDetectionTriggered triggered)
    {
        ConcurrentBag<OpenPracticeDetection> pilotDetections = GetPilotDetections(triggered.SessionId, triggered.TrackId, triggered.PilotId);

        pilotDetections.Add(new OpenPracticeDetection(triggered.DetectionId, triggered.SessionId, triggered.PilotId, null, true, triggered.OrdinalPosition, triggered.TrackDetectorCount, triggered.Lane, triggered.HardwareTime, triggered.SoftwareTime, triggered.UtcTime));
    }
    private void When(OpenPracticePilotDetectionOccured occured)
    {
        ConcurrentBag<OpenPracticeDetection> pilotDetections = GetPilotDetections(occured.SessionId, occured.TrackId, occured.PilotId);

        pilotDetections.Add(new OpenPracticeDetection(occured.DetectionId, occured.SessionId, occured.PilotId, occured.TimerId, true, occured.TrackTimerOrdinal, occured.TrackTimerTotal, occured.Lane, occured.HardwareTime, occured.SoftwareTime, occured.UtcTime));
    }

    private static TimeSpan CalculateDuration(Guid? startTimerId, ulong? startHardwareTime, long? startSoftwareTime, DateTime startUtcTime, Guid? endTimerId, ulong? endHardwareTime, long? endSoftwareTime, DateTime endUtcTime)
    {
        if (startTimerId.HasValue && endTimerId.HasValue && startTimerId == endTimerId && startHardwareTime.HasValue && endHardwareTime.HasValue)
        {
            if (endHardwareTime.Value >= startHardwareTime.Value)
            {
                return TimeSpan.FromMicroseconds(endHardwareTime.Value - startHardwareTime.Value);
            }
        }
        if (startSoftwareTime.HasValue && endSoftwareTime.HasValue && endSoftwareTime >= startSoftwareTime)
        {
            return TimeSpan.FromMilliseconds(endSoftwareTime.Value - startSoftwareTime.Value);
        }
        if (endUtcTime >= startUtcTime)
        {
            return endUtcTime - startUtcTime;
        }
        return CalculateDuration(endTimerId, endHardwareTime, endSoftwareTime, endUtcTime, startTimerId, startHardwareTime, startSoftwareTime, startUtcTime);
    }
    private static IEnumerable<OpenPracticeTimingMoment> GetTimingMoments(Guid sessionId, Guid trackId, Guid pilotId, IEnumerable<OpenPracticeDetection> pilotDetections, TimeSpan minimumLapTime, TimeSpan maximumLapTime)
    {
        if (!pilotDetections.Any())
        {
            return Enumerable.Empty<OpenPracticeTimingMoment>();
        }

        IEnumerable<OpenPracticeDetection> orderedDetections = pilotDetections.OrderBy(d => d.UtcTime);

        List<OpenPracticeTimingMoment> moments = new();

        OpenPracticeDetection? lapStartedDetection = null;
        OpenPracticeDetection? splitStartDetection = null;
        byte? currentSplit = null;
        OpenPracticeDetection? previousDetection = null;

        foreach (OpenPracticeDetection detection in orderedDetections)
        {
            if (!detection.IsValid)
            {
                moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.DetectionDiscardedAsItsInvalid));
                continue;
            }

            if (detection.TrackTimerOrdinal == 0)
            {
                // Lap start
                if (lapStartedDetection == null)
                {
                    lapStartedDetection = detection;
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.LapStarted));
                }
                else
                {
                    // Lap end
                    TimeSpan lapDuration = CalculateDuration(
                        lapStartedDetection.TimerId, lapStartedDetection.HardwareTime, lapStartedDetection.SoftwareTime, lapStartedDetection.UtcTime,
                        detection.TimerId, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
                    if (lapDuration < minimumLapTime)
                    {
                        moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.DetectionDiscardedDueToMinimiumLapTime));
                    }
                    else if (lapDuration > maximumLapTime)
                    {
                        moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.LapInvalidatedDueToMaximumLapTime));
                        moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.LapStarted));
                        lapStartedDetection = detection; // Start a new lap from this detection
                    }
                    else
                    {
                        moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.LapCompleted));
                        moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.LapStarted));
                        lapStartedDetection = detection; // Start a new lap from this detection
                    }
                }
                // Reset split tracking for new lap
                splitStartDetection = null;
                currentSplit = null;
            }
            else
            {
                // Split handling can be added here if needed
                if (currentSplit != null && detection.TrackTimerOrdinal == currentSplit + 1)
                {
                    // Split completed
                    TimeSpan splitDuration = CalculateDuration(
                        splitStartDetection!.TimerId, splitStartDetection.HardwareTime, splitStartDetection.SoftwareTime, splitStartDetection.UtcTime,
                        detection.TimerId, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.SplitCompleted));
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.SplitStarted));
                    splitStartDetection = detection;
                    currentSplit = detection.TrackTimerOrdinal;
                }
                else if (currentSplit == null && detection.TrackTimerOrdinal == 1)
                {
                    // First split started
                    splitStartDetection = detection;
                    currentSplit = detection.TrackTimerOrdinal;
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.SplitStarted));

                }
                else
                {
                    // Split skipped or out of order
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.SplitSkipped));
                }
            }
            previousDetection = detection;
        }

        if(lapStartedDetection != null)
        {
            TimeSpan lapDuration = CalculateDuration(
                        lapStartedDetection.TimerId, lapStartedDetection.HardwareTime, lapStartedDetection.SoftwareTime, lapStartedDetection.UtcTime,
                        null, null, null, DateTime.UtcNow);

            if(lapDuration > maximumLapTime)
            {
                moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, null, OpenPracticeTimingMoment.TimingMomentType.LapInvalidatedDueToMaximumLapTime));
            }
        }
        return moments;
    }
    private static IEnumerable<IEnumerable<OpenPracticeLap>> GetPilotSessionLapGroups(Guid sessionId, Guid trackId, Guid pilotId,IDictionary<Guid,OpenPracticeDetection> pilotDetections, IEnumerable<OpenPracticeTimingMoment> moments)
    {
        if (!pilotDetections.Any())
        {
            return Enumerable.Empty<IEnumerable<OpenPracticeLap>>();
        }

        List<OpenPracticeLap> laps = new();
        OpenPracticeDetection? lapStartDetection = null;
        foreach (OpenPracticeTimingMoment moment in moments)
        {
            if (!moment.MomentId.HasValue)
            {
                continue;
            }

            if (moment.Type == OpenPracticeTimingMoment.TimingMomentType.LapStarted)
            {
                lapStartDetection = pilotDetections[moment.MomentId.Value];
            }
            else if (moment.Type == OpenPracticeTimingMoment.TimingMomentType.LapCompleted && lapStartDetection != null)
            {
                OpenPracticeDetection lapEndDetection = pilotDetections[moment.MomentId.Value];
                TimeSpan lapDuration = CalculateDuration(
                    lapStartDetection.TimerId, lapStartDetection.HardwareTime, lapStartDetection.SoftwareTime, lapStartDetection.UtcTime,
                    lapEndDetection.TimerId, lapEndDetection.HardwareTime, lapEndDetection.SoftwareTime, lapEndDetection.UtcTime);
                laps.Add(new OpenPracticeLap(sessionId, trackId, pilotId, lapStartDetection, lapEndDetection, lapDuration));
                lapStartDetection = null;
            }
        }
        return GetConsecutiveLapGroups(laps);
    }
    private static IEnumerable<IEnumerable<OpenPracticeLap>> GetConsecutiveLapGroups(IEnumerable<OpenPracticeLap> laps)
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
    private static Dictionary<uint, OpenPracticeLapRecord> GetPilotRecords(Guid sessionId, Guid trackId, Guid pilotId, IEnumerable<IEnumerable<OpenPracticeLap>> lapGroups)
    {
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
        foreach (var lapCount in sessionRecords.Keys.ToList())
        {
            sessionRecords[lapCount].Sort((x, y) => x.Record.CompareTo(y.Record));
        }
        return sessionRecords.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<OpenPracticeLapRecord>)kvp.Value);
    }

    public OpenPracticeSessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime)
    {
        ConcurrentDictionary<Guid, ConcurrentBag<OpenPracticeDetection>> trackPilotsDetections = GetSessionDetections(sessionId, trackId);

        Dictionary<Guid, IEnumerable<OpenPracticeTimingMoment>> allMoments = new();
        Dictionary<Guid, IDictionary<Guid,OpenPracticeDetection>> allDetections = new();
        Dictionary<Guid, IEnumerable<IEnumerable<OpenPracticeLap>>> pilotLaps = new();
        Dictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> allRecords = new();

        foreach (KeyValuePair<Guid, ConcurrentBag<OpenPracticeDetection>> pilotEntry in trackPilotsDetections)
        {
            Guid pilotId = pilotEntry.Key;

            IDictionary<Guid,OpenPracticeDetection> pilotDetections = pilotEntry.Value.ToDictionary(x=>x.Id);
            allDetections[pilotId] = pilotDetections;
            IEnumerable<OpenPracticeTimingMoment> pilotMoments = GetTimingMoments(sessionId, trackId, pilotId, pilotDetections.Values, minimumLapTime, maximumLapTime);
            allMoments[pilotId] = pilotMoments;
            IEnumerable<IEnumerable<OpenPracticeLap>> pilotLapGroups = GetPilotSessionLapGroups(sessionId, trackId, pilotId, pilotDetections,pilotMoments);
            pilotLaps[pilotId] = pilotLapGroups;
            allRecords[pilotId] = GetPilotRecords(sessionId, trackId, pilotId, pilotLapGroups);
        }

        Dictionary<uint, IEnumerable<OpenPracticeLapRecord>> sessionRecords = GetSessionRecords(allRecords);

        return new OpenPracticeSessionTimingInformation(allMoments, allDetections, pilotLaps, allRecords, sessionRecords);
    }
    public OpenPracticeSessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId, TimeSpan minimumLapTime, TimeSpan maximumLapTime)
    {
        ConcurrentBag<OpenPracticeDetection> detections = GetPilotDetections(sessionId, trackId, pilotId);
        IEnumerable<OpenPracticeTimingMoment> moments = GetTimingMoments(sessionId, trackId, pilotId, detections, minimumLapTime, maximumLapTime);
        IDictionary<Guid,OpenPracticeDetection> pilotDetections = detections.ToDictionary(x=>x.Id);
        IEnumerable<IEnumerable<OpenPracticeLap>> pilotLaps = GetPilotSessionLapGroups(sessionId, trackId, pilotId, pilotDetections,moments);
        IDictionary<uint, OpenPracticeLapRecord> pilotRecords = GetPilotRecords(sessionId, trackId, pilotId, pilotLaps);

        return new OpenPracticeSessionPilotTimingInfo(moments,pilotDetections, pilotLaps, pilotRecords);
    }
    public OpenPracticeDetection? GetLastPilotDetection(Guid sessionId, Guid trackId, Guid pilotId)
    {
        ConcurrentBag<OpenPracticeDetection> detections = GetPilotDetections(sessionId, trackId, pilotId);
        OpenPracticeDetection? lastDetection = detections.OrderByDescending(d => d.UtcTime).FirstOrDefault();
        return lastDetection;
    }
}
