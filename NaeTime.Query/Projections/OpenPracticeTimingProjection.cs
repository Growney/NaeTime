using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class OpenPracticeTimingProjection : IOpenPracticeTimingProjection
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ConcurrentDictionary<Guid,OpenPracticeDetection>>>> _sessionTrackPilotDetections = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, OpenPracticeDetection>> _sessionPilotLastDetection = new();
    private readonly ConcurrentDictionary<Guid, TimeSpan> _minimumLapTimes = new();
    private readonly ConcurrentDictionary<Guid, TimeSpan> _maximumLapTimes = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, TimeSpan>> _sessionPilotMinimumLapTimes = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, TimeSpan>> _sessionPilotMaximumLap = new();

    private ConcurrentDictionary<Guid, ConcurrentDictionary<Guid,OpenPracticeDetection>> GetSessionDetections(Guid sessionId, Guid trackId)
    {
        ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ConcurrentDictionary<Guid,OpenPracticeDetection>>> sessionRecords = _sessionTrackPilotDetections.GetOrAdd(sessionId, _ => new());
        return sessionRecords.GetOrAdd(trackId, _ => new());
    }
    private ConcurrentDictionary<Guid,OpenPracticeDetection> GetPilotDetections(Guid sessionId, Guid trackId, Guid pilotId)
    {
        ConcurrentDictionary<Guid, ConcurrentDictionary<Guid,OpenPracticeDetection>> trackRecords = GetSessionDetections(sessionId, trackId);
        return trackRecords.GetOrAdd(pilotId, _ => new());
    }

    private void When(OpenPracticePilotLastDetectionRevised revised)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> sessionLastDetections = _sessionPilotLastDetection.GetOrAdd(revised.SessionId, _ => new());
        ConcurrentDictionary<Guid, OpenPracticeDetection> detections = GetPilotDetections(revised.SessionId, revised.TrackId, revised.PilotId);
        sessionLastDetections[revised.PilotId] = detections[revised.DetectionId];
    }
    private void When(OpenPracticePilotDowned downed)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> sessionLastDetections = _sessionPilotLastDetection.GetOrAdd(downed.SessionId, _ => new());
        sessionLastDetections.TryRemove(downed.PilotId, out _);
    }

    private void When(OpenPracticePilotDetectionTriggered triggered)
    {
        ConcurrentDictionary<Guid,OpenPracticeDetection> pilotDetections = GetPilotDetections(triggered.SessionId, triggered.TrackId, triggered.PilotId);

        pilotDetections.TryAdd(triggered.DetectionId,new OpenPracticeDetection(triggered.DetectionId, triggered.SessionId, triggered.PilotId, null, true, triggered.OrdinalPosition, triggered.TrackDetectorCount, triggered.Lane, triggered.HardwareTime, triggered.SoftwareTime, triggered.UtcTime,null,null));
    }
    private void When(OpenPracticePilotDetectionOccured occured)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> pilotDetections = GetPilotDetections(occured.SessionId, occured.TrackId, occured.PilotId);

        pilotDetections.TryAdd(occured.DetectionId,new OpenPracticeDetection(occured.DetectionId, occured.SessionId, occured.PilotId, occured.TimerId, true, occured.TrackTimerOrdinal, occured.TrackTimerTotal, occured.Lane, occured.HardwareTime, occured.SoftwareTime, occured.UtcTime,null,null));
    }
    private void When(OpenPracticePilotDetectionInvalidated invalidated)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> pilotDetections = GetPilotDetections(invalidated.SessionId, invalidated.TrackId, invalidated.PilotId);

        if (!pilotDetections.ContainsKey(invalidated.DetectionId))
        {
            return;
        }

        pilotDetections[invalidated.DetectionId] = pilotDetections[invalidated.DetectionId] with { IsValid = false };
    }
    private void When(OpenPracticePilotDetectionValidated validated)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> pilotDetections = GetPilotDetections(validated.SessionId, validated.TrackId, validated.PilotId);

        if (!pilotDetections.ContainsKey(validated.DetectionId))
        {
            return;
        }
        pilotDetections[validated.DetectionId] = pilotDetections[validated.DetectionId] with { IsValid = true };
    }
    private void When(OpenPracticePilotPackEndInsertedAfterDetection inserted)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> pilotDetections = GetPilotDetections(inserted.SessionId, inserted.TrackId, inserted.PilotId);
        if (!pilotDetections.ContainsKey(inserted.DetectionId))
        {
            return;
        }
        pilotDetections[inserted.DetectionId] = pilotDetections[inserted.DetectionId] with { PackEndAfter = inserted.PackEndId };
    }
    private void When(OpenPracticePilotPackEndInsertedBeforeDetection inserted)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> pilotDetections = GetPilotDetections(inserted.SessionId, inserted.TrackId, inserted.PilotId);
        if (!pilotDetections.ContainsKey(inserted.DetectionId))
        {
            return;
        }
        pilotDetections[inserted.DetectionId] = pilotDetections[inserted.DetectionId] with { PackEndBefore = inserted.PackEndId };
    }
    private void When(OpenPracticePilotPackEndRemoved removed)
    {
        ConcurrentDictionary<Guid, OpenPracticeDetection> pilotDetections = GetPilotDetections(removed.SessionId, removed.TrackId, removed.PilotId);
        if (!pilotDetections.ContainsKey(removed.DetectionId))
        {
            return;
        }
        OpenPracticeDetection detection = pilotDetections[removed.DetectionId];
        if (detection.PackEndAfter == removed.PackEndId)
        {
            pilotDetections[removed.DetectionId] = detection with { PackEndAfter = null };
        }
        else if (detection.PackEndBefore == removed.PackEndId)
        {
            pilotDetections[removed.DetectionId] = detection with { PackEndBefore = null };
        }
    }
    private void When(OpenPracticeSessionScheduled scheduled)
    {
        if(scheduled.MinimumLapTime.HasValue)
        {
            _minimumLapTimes[scheduled.SessionId] = scheduled.MinimumLapTime.Value;
        }
        if(scheduled.MaximumLapTime.HasValue)
        {
            _maximumLapTimes[scheduled.SessionId] = scheduled.MaximumLapTime.Value;
        }
    }
    private void When(OpenPracticeSessionMinimumLapTimeSet set)
    {
        _minimumLapTimes[set.SessionId] = set.MinimumLapTime;
    }
    private void When(OpenPracticeSessionMaximumLapTimeSet set)
    {
        _maximumLapTimes[set.SessionId] = set.MaximumLapTime;
    }
    private void When(OpenPracticeSessionMinimumLapTimeReset reset)
    {
        _minimumLapTimes.TryRemove(reset.SessionId, out _);
    }
    private void When(OpenPracticeSessionMaximumLapTimeReset reset)
    {
        _maximumLapTimes.TryRemove(reset.SessionId, out _);
    }
    private void When(OpenPracticeSessionPilotMinimumLapTimeReset pilotMinimumLapTimeReset)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotMinimumLapTimes = _sessionPilotMinimumLapTimes.GetOrAdd(pilotMinimumLapTimeReset.SessionId, _ => new());
        pilotMinimumLapTimes.TryRemove(pilotMinimumLapTimeReset.PilotId, out _);
    }
    private void When(OpenPracticeSessionPilotMinimumLapTimeSet pilotMinimumLapTimeSet)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotMinimumLapTimes = _sessionPilotMinimumLapTimes.GetOrAdd(pilotMinimumLapTimeSet.SessionId, _ => new());
        pilotMinimumLapTimes[pilotMinimumLapTimeSet.PilotId] = pilotMinimumLapTimeSet.MinimumLapTime;
    }
    private void When(OpenPracticeSessionPilotMaximumLapTimeReset pilotMaximumLapTimeReset)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotMaximumLapTimes = _sessionPilotMaximumLap.GetOrAdd(pilotMaximumLapTimeReset.SessionId, _ => new());
        pilotMaximumLapTimes.TryRemove(pilotMaximumLapTimeReset.PilotId, out _);
    }
    private void When(OpenPracticeSessionPilotMaximumLapTimeSet pilotMaximumLapTimeSet)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotMaximumLapTimes = _sessionPilotMaximumLap.GetOrAdd(pilotMaximumLapTimeSet.SessionId, _ => new());
        pilotMaximumLapTimes[pilotMaximumLapTimeSet.PilotId] = pilotMaximumLapTimeSet.MaximumLapTime;
    }

    private TimeSpan GetMinimumLapTime(Guid sessionId, Guid pilotId)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotMinimumLapTimes = _sessionPilotMinimumLapTimes.GetOrAdd(sessionId, _ => new());
        if (pilotMinimumLapTimes.TryGetValue(pilotId, out TimeSpan pilotMinLapTime))
        {
            return pilotMinLapTime;
        }
        if (_minimumLapTimes.TryGetValue(sessionId, out TimeSpan sessionMinLapTime))
        {
            return sessionMinLapTime;
        }
        return TimeSpan.Zero;
    }
    private TimeSpan GetMaximumLapTime(Guid sessionId, Guid pilotId)
    {
        ConcurrentDictionary<Guid, TimeSpan> pilotMaximumLapTimes = _sessionPilotMaximumLap.GetOrAdd(sessionId, _ => new());
        if (pilotMaximumLapTimes.TryGetValue(pilotId, out TimeSpan pilotMaxLapTime))
        {
            return pilotMaxLapTime;
        }
        if (_maximumLapTimes.TryGetValue(sessionId, out TimeSpan sessionMaxLapTime))
        {
            return sessionMaxLapTime;
        }
        return TimeSpan.FromMinutes(1); // Default maximum lap time
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
    private IEnumerable<OpenPracticeTimingMoment> GetTimingMoments(Guid sessionId, Guid trackId, Guid pilotId, IEnumerable<OpenPracticeDetection> pilotDetections)
    {
        if (!pilotDetections.Any())
        {
            return Enumerable.Empty<OpenPracticeTimingMoment>();
        }

        TimeSpan minimumLapTime = GetMinimumLapTime(sessionId, pilotId);
        TimeSpan maximumLapTime = GetMaximumLapTime(sessionId, pilotId);

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

            if (detection.PackEndBefore.HasValue)
            {
                if(lapStartedDetection != null)
                {
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.PackEndBefore, OpenPracticeTimingMoment.TimingMomentType.LapStoppedByEndOfPack));
                }
                else
                {
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.PackEndBefore, OpenPracticeTimingMoment.TimingMomentType.EndOfPack));
                }
                // Reset lap and split tracking
                lapStartedDetection = null;
                splitStartDetection = null;
                currentSplit = null;
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
                        lapStartedDetection = detection; // Start a new lap from this detection
                    }
                    else
                    {
                        moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.Id, OpenPracticeTimingMoment.TimingMomentType.LapCompleted));
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
            if(detection.PackEndAfter.HasValue)
            {
                if(lapStartedDetection != null)
                {
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.PackEndAfter, OpenPracticeTimingMoment.TimingMomentType.LapStoppedByEndOfPack));
                }
                else
                {
                    moments.Add(new OpenPracticeTimingMoment(sessionId, trackId, pilotId, detection.PackEndAfter, OpenPracticeTimingMoment.TimingMomentType.EndOfPack));
                }
                // Reset lap and split tracking
                lapStartedDetection = null;
                splitStartDetection = null;
                currentSplit = null;
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

            if (moment.Type is OpenPracticeTimingMoment.TimingMomentType.LapStarted or OpenPracticeTimingMoment.TimingMomentType.LapInvalidatedDueToMaximumLapTime)
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
                lapStartDetection = pilotDetections[moment.MomentId.Value];
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

    public OpenPracticeSessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId)
    {
        ConcurrentDictionary<Guid, ConcurrentDictionary<Guid,OpenPracticeDetection>> trackPilotsDetections = GetSessionDetections(sessionId, trackId);

        Dictionary<Guid, IEnumerable<OpenPracticeTimingMoment>> allMoments = new();
        Dictionary<Guid, IDictionary<Guid,OpenPracticeDetection>> allDetections = new();
        Dictionary<Guid, IEnumerable<IEnumerable<OpenPracticeLap>>> pilotLaps = new();
        Dictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> allRecords = new();

        foreach (KeyValuePair<Guid, ConcurrentDictionary<Guid,OpenPracticeDetection>> pilotEntry in trackPilotsDetections)
        {
            Guid pilotId = pilotEntry.Key;

            IDictionary<Guid,OpenPracticeDetection> pilotDetections = pilotEntry.Value;
            allDetections[pilotId] = pilotDetections;
            IEnumerable<OpenPracticeTimingMoment> pilotMoments = GetTimingMoments(sessionId, trackId, pilotId, pilotDetections.Values);
            allMoments[pilotId] = pilotMoments;
            IEnumerable<IEnumerable<OpenPracticeLap>> pilotLapGroups = GetPilotSessionLapGroups(sessionId, trackId, pilotId, pilotDetections,pilotMoments);
            pilotLaps[pilotId] = pilotLapGroups;
            allRecords[pilotId] = GetPilotRecords(sessionId, trackId, pilotId, pilotLapGroups);
        }

        Dictionary<uint, IEnumerable<OpenPracticeLapRecord>> sessionRecords = GetSessionRecords(allRecords);

        return new OpenPracticeSessionTimingInformation(allMoments, allDetections, pilotLaps, allRecords, sessionRecords);
    }
    public OpenPracticeSessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId)
    {
        IEnumerable<OpenPracticeDetection> detections = GetPilotDetections(sessionId, trackId, pilotId).Values;
        IEnumerable<OpenPracticeTimingMoment> moments = GetTimingMoments(sessionId, trackId, pilotId, detections);
        IDictionary<Guid,OpenPracticeDetection> pilotDetections = detections.ToDictionary(x=>x.Id);
        IEnumerable<IEnumerable<OpenPracticeLap>> pilotLaps = GetPilotSessionLapGroups(sessionId, trackId, pilotId, pilotDetections,moments);
        IDictionary<uint, OpenPracticeLapRecord> pilotRecords = GetPilotRecords(sessionId, trackId, pilotId, pilotLaps);

        return new OpenPracticeSessionPilotTimingInfo(moments,pilotDetections, pilotLaps, pilotRecords);
    }
    public OpenPracticeDetection? GetLastPilotDetection(Guid sessionId, Guid trackId, Guid pilotId)
    {
        if(!_sessionPilotLastDetection.TryGetValue(sessionId, out var pilotDetections))
        {
            return null;
        }
        if(!pilotDetections.TryGetValue(pilotId,out var detection))
        {
            return null;
        }
        return detection;
    }
    public IEnumerable<OpenPracticeTimingMoment> GetPilotMoments(Guid sessionId, Guid trackId, Guid pilotId)
    {
        IEnumerable<OpenPracticeDetection> detections = GetPilotDetections(sessionId, trackId, pilotId).Values;
        return GetTimingMoments(sessionId, trackId, pilotId, detections);
    }
}
