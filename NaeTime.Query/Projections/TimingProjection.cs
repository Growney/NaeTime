using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;

public class TimingProjection : ITimingProjection
{
    private readonly ConcurrentDictionary<Guid, Guid> _sessionTracks = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, byte>> _trackDetectors = new();
    private readonly ConcurrentDictionary<Guid, OpenPracticePilotPackEnd> _packEnds = new();
    private readonly ConcurrentDictionary<Guid, Detection> _detections = new();
    private Guid? _activeSession;
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, Guid>> _pilotLanes = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, bool>> _laneStatus = new();
    private readonly ConcurrentDictionary<Guid, TimeSpan> _minimumLapTimes = new();
    private readonly ConcurrentDictionary<Guid, TimeSpan> _maximumLapTimes = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, TimeSpan>> _sessionPilotMinimumLapTimes = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, TimeSpan>> _sessionPilotMaximumLap = new();

    private void When(TrackDesigned designed)
    {
        ConcurrentDictionary<Guid, byte> detectors = new();

        _trackDetectors[designed.TrackId] = detectors;

        for (byte i = 0; i < designed.DetectorIds.Length; i++)
        {
            detectors[designed.DetectorIds[i]] = i;
        }
    }
    private void When(TrackDetectorAdded added)
    {
        if (!_trackDetectors.TryGetValue(added.TrackId, out var detectors))
        {
            return;
        }

        detectors[added.DetectorId] = added.OrdinalPosition;
    }
    private void When(TrackDetectorMoved moved)
    {
        if (!_trackDetectors.TryGetValue(moved.TrackId, out var detectors))
        {
            return;
        }

        detectors[moved.DetectorId] = moved.OrdinalPosition;
    }
    private void When(TrackDetectorRemoved removed)
    {
        if (!_trackDetectors.TryGetValue(removed.TrackId, out var detectors))
        {
            return;
        }

        detectors.Remove(removed.TrackId, out var _);
    }
    private void When(OpenPracticePilotPackEndAdded added)
    {
        _packEnds[added.PackEndId] = new OpenPracticePilotPackEnd(added.PackEndId, added.SessionId, added.PilotId, added.SoftwareTime, added.UtcTime);
    }
    private void When(OpenPracticePilotPackEndRemoved removed)
    {
        _packEnds.Remove(removed.PackEndId, out var _);
    }
    private void When(OpenPracticeSessionScheduled scheduled)
    {
        if (scheduled.MinimumLapTime.HasValue)
        {
            _minimumLapTimes[scheduled.SessionId] = scheduled.MinimumLapTime.Value;
        }
        if (scheduled.MaximumLapTime.HasValue)
        {
            _maximumLapTimes[scheduled.SessionId] = scheduled.MaximumLapTime.Value;
        }

        _sessionTracks[scheduled.SessionId] = scheduled.TrackId;
    }
    private void When(OpenPracticeSessionTrackChanged changed)
    {
        _sessionTracks[changed.SessionId] = changed.NewTrackId;
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
    private (Guid? sessionID, Guid? pilotId, bool? isLaneEnabled, Guid? trackId) GetActiveDetails(byte lane)
    {
        if (_activeSession == null)
        {
            return (null, null, null, null);
        }
        Guid? trackId = null;
        if (_sessionTracks.TryGetValue(_activeSession.Value, out var storedTrackId))
        {
            trackId = storedTrackId;
        }

        if (!_laneStatus.TryGetValue(_activeSession.Value, out var statuses))
        {
            return (_activeSession, null, null, trackId);
        }

        statuses.TryGetValue(lane, out var isEnabled);

        if (!_pilotLanes.TryGetValue(_activeSession.Value, out var pilots) || !pilots.TryGetValue(lane, out var pilotId))
        {
            return (_activeSession, null, isEnabled, trackId);
        }

        return (_activeSession, pilotId, isEnabled, trackId);
    }
    private void When(HardwareDetectionOccured occured)
    {
        (Guid? sessionId, Guid? pilotId, bool? isLaneEnabled, Guid? trackId) = GetActiveDetails(occured.Lane);

        _detections[occured.Id] = new Detection(occured.Id, sessionId, trackId, pilotId, occured.TimerId, isLaneEnabled ?? true, occured.Lane, occured.HardwareTime, occured.SoftwareTime, occured.UtcTime);
    }
    private void When(DetectionManuallyTriggered triggered)
    {
        (Guid? sessionId, Guid? pilotId, bool? isLaneEnabled, Guid? trackId) = GetActiveDetails(triggered.Lane);

        _detections[triggered.Id] = new Detection(triggered.Id, sessionId, trackId, pilotId, triggered.TimerId, isLaneEnabled ?? true, triggered.Lane, null, triggered.SoftwareTime, triggered.UtcTime);
    }
    private void When(DetectionSessionOverridden overridden)
    {
        if (!_detections.TryGetValue(overridden.Id, out var detection))
        {
            return;
        }

        Guid? trackId = detection.TrackId;
        if (_sessionTracks.TryGetValue(overridden.SessionId, out var sessionTrackId))
        {
            trackId = sessionTrackId;
        }

        _detections[overridden.Id] = detection with
        {
            SessionId = overridden.SessionId,
            TrackId = trackId,
        };
    }
    private void When(DetectionPilotOverridden overridden)
    {
        if (!_detections.TryGetValue(overridden.Id, out var detection))
        {
            return;
        }

        _detections[overridden.Id] = detection with { PilotId = overridden.PilotId };
    }
    private void When(DetectionStatusSet statusSet)
    {
        if (!_detections.TryGetValue(statusSet.Id, out var detection))
        {
            return;
        }

        _detections[statusSet.Id] = detection with { IsValid = statusSet.IsValid };
    }
    private void When(DetectionMoved moved)
    {
        if (!_detections.TryGetValue(moved.Id, out var detection))
        {
            return;
        }

        _detections[moved.Id] = detection with
        {
            SoftwareTime = moved.SoftwareTime,
            UtcTime = moved.UtcTime,
        };
    }
    private void When(OpenPracticeSessionActivated activated)
    {
        _activeSession = activated.SessionId;
    }
    private void When(OpenPracticeSessionDeactivated _)
    {
        _activeSession = null;
    }
    private void When(OpenPracticeSessionLanePilotSet set)
    {
        _pilotLanes[set.SessionId][set.Lane] = set.PilotId;
    }
    private void When(OpenPracticeSessionLanePilotReset reset)
    {
        _pilotLanes[reset.SessionId].Remove(reset.Lane, out var _);
    }
    private void When(OpenPracticeSessionLaneConfigured configured)
    {
        _laneStatus[configured.SessionId][configured.Lane] = true;
    }
    private void When(OpenPracticeSessionLaneStatusSet configured)
    {
        _laneStatus[configured.SessionId][configured.Lane] = configured.IsEnabled;
    }
    private Dictionary<Guid, Dictionary<Guid, Detection>> GetSessionDetections(Guid sessionId, Guid trackId)
    {
        var selectedDetections = _detections.Values.Where(x => x.SessionId == sessionId && x.TrackId == trackId);

        Dictionary<Guid, Dictionary<Guid, Detection>> results = new();

        foreach(var detection in selectedDetections)
        {
            if (!detection.PilotId.HasValue)
            {
                continue;
            }

            if(!results.TryGetValue(detection.PilotId.Value,out var detections))
            {
                detections = new();
                results.Add(detection.PilotId.Value, detections);
            }

            detections[detection.Id] = detection;
        }

        return results;
    }
    private IEnumerable<Detection> GetPilotDetections(Guid sessionId, Guid trackId, Guid pilotId)
        => _detections.Values.Where(x => x.SessionId == sessionId && x.TrackId == trackId && x.PilotId == pilotId);
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
    private IEnumerable<TimingMoment> GetTimingMoments(Guid sessionId, Guid trackId, Guid pilotId, IEnumerable<Detection> pilotDetections)
    {
        if (!pilotDetections.Any())
        {
            return Enumerable.Empty<TimingMoment>();
        }

        IEnumerable<OpenPracticePilotPackEnd> packEnds = _packEnds.Values.Where(x => x.PilotId == pilotId && x.SessionId == sessionId);

        TimeSpan minimumLapTime = GetMinimumLapTime(sessionId, pilotId);
        TimeSpan maximumLapTime = GetMaximumLapTime(sessionId, pilotId);

        IEnumerable<ITimingOccasion> orderedOccasions = pilotDetections.AsEnumerable<ITimingOccasion>().Union(packEnds).OrderBy(d => d.UtcTime);

        List<TimingMoment> moments = new();

        Detection? lapStartedDetection = null;
        Detection? splitStartDetection = null;
        byte? currentSplit = null;
        Detection? previousDetection = null;

        foreach (ITimingOccasion occasion in orderedOccasions)
        {
            switch (occasion)
            {
                case OpenPracticePilotPackEnd packEnd:
                    {
                        if (lapStartedDetection != null)
                        {
                            moments.Add(new TimingMoment(sessionId, trackId, pilotId, packEnd.Id, TimingMoment.TimingMomentType.LapStoppedByEndOfPack));
                        }
                        else
                        {
                            moments.Add(new TimingMoment(sessionId, trackId, pilotId, packEnd.Id, TimingMoment.TimingMomentType.EndOfPack));
                        }
                        // Reset lap and split tracking
                        lapStartedDetection = null;
                        splitStartDetection = null;
                        currentSplit = null;
                    }
                    break;
                case Detection detection:
                    {
                        if (!detection.IsValid)
                        {
                            moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.DetectionDiscardedAsItsInvalid));
                            continue;
                        }

                        if(!detection.TrackId.HasValue || !_trackDetectors.TryGetValue(detection.TrackId.Value, out var trackOrdinals))
                        {
                            continue;
                        }

                        if(!detection.TimerId.HasValue || !trackOrdinals.TryGetValue(detection.TimerId.Value, out byte trackOrdinal))
                        {
                            continue;
                        }

                        if (trackOrdinal == 0)
                        {
                            // Lap start
                            if (lapStartedDetection == null)
                            {
                                lapStartedDetection = detection;
                                moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.LapStarted));
                            }
                            else
                            {
                                // Lap end
                                TimeSpan lapDuration = CalculateDuration(
                                    lapStartedDetection.TimerId, lapStartedDetection.HardwareTime, lapStartedDetection.SoftwareTime, lapStartedDetection.UtcTime,
                                    detection.TimerId, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
                                if (lapDuration < minimumLapTime)
                                {
                                    moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.DetectionDiscardedDueToMinimiumLapTime));
                                }
                                else if (lapDuration > maximumLapTime)
                                {
                                    moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.LapInvalidatedDueToMaximumLapTime));
                                    lapStartedDetection = detection; // Start a new lap from this detection
                                }
                                else
                                {
                                    moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.LapCompleted));
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
                            if (currentSplit != null && trackOrdinal == currentSplit + 1)
                            {
                                // Split completed
                                TimeSpan splitDuration = CalculateDuration(
                                    splitStartDetection!.TimerId, splitStartDetection.HardwareTime, splitStartDetection.SoftwareTime, splitStartDetection.UtcTime,
                                    detection.TimerId, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
                                moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.SplitCompleted));
                                moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.SplitStarted));
                                splitStartDetection = detection;
                                currentSplit = trackOrdinal;
                            }
                            else if (currentSplit == null && trackOrdinal == 1)
                            {
                                // First split started
                                splitStartDetection = detection;
                                currentSplit = trackOrdinal;
                                moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.SplitStarted));

                            }
                            else
                            {
                                // Split skipped or out of order
                                moments.Add(new TimingMoment(sessionId, trackId, pilotId, detection.Id, TimingMoment.TimingMomentType.SplitSkipped));
                            }
                        }
                        previousDetection = detection;
                    }
                    break;
                default:
                    break;
            }
            
        }

        if (lapStartedDetection != null)
        {
            TimeSpan lapDuration = CalculateDuration(
                        lapStartedDetection.TimerId, lapStartedDetection.HardwareTime, lapStartedDetection.SoftwareTime, lapStartedDetection.UtcTime,
                        null, null, null, DateTime.UtcNow);

            if (lapDuration > maximumLapTime)
            {
                moments.Add(new TimingMoment(sessionId, trackId, pilotId, null, TimingMoment.TimingMomentType.LapInvalidatedDueToMaximumLapTime));
            }
        }
        return moments;
    }
    private static IEnumerable<IEnumerable<OpenPracticeLap>> GetPilotSessionLapGroups(Guid sessionId, Guid trackId, Guid pilotId, IDictionary<Guid, Detection> pilotDetections, IEnumerable<TimingMoment> moments)
    {
        if (!pilotDetections.Any())
        {
            return Enumerable.Empty<IEnumerable<OpenPracticeLap>>();
        }

        List<OpenPracticeLap> laps = new();
        Detection? lapStartDetection = null;
        foreach (TimingMoment moment in moments)
        {
            if (!moment.MomentId.HasValue)
            {
                continue;
            }

            if (moment.Type is TimingMoment.TimingMomentType.LapStarted or TimingMoment.TimingMomentType.LapInvalidatedDueToMaximumLapTime)
            {
                lapStartDetection = pilotDetections[moment.MomentId.Value];
            }
            else if (moment.Type == TimingMoment.TimingMomentType.LapCompleted && lapStartDetection != null)
            {
                Detection lapEndDetection = pilotDetections[moment.MomentId.Value];
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

    public SessionTimingInformation GetSessionTimingInfo(Guid sessionId, Guid trackId)
    {
        Dictionary<Guid, Dictionary<Guid, Detection>> trackPilotsDetections = GetSessionDetections(sessionId, trackId);

        Dictionary<Guid, IEnumerable<TimingMoment>> allMoments = new();
        Dictionary<Guid, IDictionary<Guid, Detection>> allDetections = new();
        Dictionary<Guid, IEnumerable<IEnumerable<OpenPracticeLap>>> pilotLaps = new();
        Dictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> allRecords = new();

        foreach (KeyValuePair<Guid, Dictionary<Guid, Detection>> pilotEntry in trackPilotsDetections)
        {
            Guid pilotId = pilotEntry.Key;

            IDictionary<Guid, Detection> pilotDetections = pilotEntry.Value;
            allDetections[pilotId] = pilotDetections;
            IEnumerable<TimingMoment> pilotMoments = GetTimingMoments(sessionId, trackId, pilotId, pilotDetections.Values);
            allMoments[pilotId] = pilotMoments;
            IEnumerable<IEnumerable<OpenPracticeLap>> pilotLapGroups = GetPilotSessionLapGroups(sessionId, trackId, pilotId, pilotDetections, pilotMoments);
            pilotLaps[pilotId] = pilotLapGroups;
            allRecords[pilotId] = GetPilotRecords(sessionId, trackId, pilotId, pilotLapGroups);
        }

        Dictionary<uint, IEnumerable<OpenPracticeLapRecord>> sessionRecords = GetSessionRecords(allRecords);

        return new SessionTimingInformation(allMoments, allDetections, pilotLaps, allRecords, sessionRecords);
    }
    public SessionPilotTimingInfo GetSessionPilotTimingInfo(Guid sessionId, Guid trackId, Guid pilotId)
    {
        IEnumerable<Detection> detections = GetPilotDetections(sessionId, trackId, pilotId);
        IEnumerable<TimingMoment> moments = GetTimingMoments(sessionId, trackId, pilotId, detections);
        IDictionary<Guid, Detection> pilotDetections = detections.ToDictionary(x => x.Id);
        IEnumerable<IEnumerable<OpenPracticeLap>> pilotLaps = GetPilotSessionLapGroups(sessionId, trackId, pilotId, pilotDetections, moments);
        IDictionary<uint, OpenPracticeLapRecord> pilotRecords = GetPilotRecords(sessionId, trackId, pilotId, pilotLaps);

        return new SessionPilotTimingInfo(moments, pilotDetections, pilotLaps, pilotRecords);
    }
    public Detection? GetLastPilotDetection(Guid sessionId, Guid trackId, Guid pilotId) => GetPilotDetections(sessionId, trackId, pilotId).OrderByDescending(x => x.UtcTime).FirstOrDefault();
    public IEnumerable<TimingMoment> GetPilotMoments(Guid sessionId, Guid trackId, Guid pilotId)
    {
        IEnumerable<Detection> detections = GetPilotDetections(sessionId, trackId, pilotId);
        return GetTimingMoments(sessionId, trackId, pilotId, detections);
    }

    public Detection? GetDetection(Guid detectionId)
    {
        if (_detections.TryGetValue(detectionId, out var detection))
        {
            return detection;
        }
        return null;
    }
}
