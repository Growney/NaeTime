using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class OpenPracticePilotTiming : AggregateRoot<OpenPracticePilotTiming.OpenPracticePilotTimingId>
{
    public class OpenPracticePilotTimingId
    {
        public Guid PilotId { get; init; }
        public Guid SessionId { get; init; }
        public Guid TrackId { get; init; }
        public override string ToString() => $"{SessionId:N}-{TrackId:N}-{PilotId:N}";
    }

    private class Detection
    {
        public Guid Id { get; init; }
        public Guid? TimerId { get; init; }
        public byte Lane { get; init; }
        public byte TrackTimerOrdinal { get; init; }
        public byte TrackTimerTotal { get; init; }
        public ulong? HardwareTime { get; init; }
        public long SoftwareTime { get; init; }
        public DateTime UtcTime { get; init; }

    }
    private class Lap
    {
        public Guid Id { get; init; }
        public required Detection StartDetection { get; init; }
        public Detection? EndDetection { get; set; }
        public List<Split> Splits { get; } = [];
        public TimeSpan? Duration { get; set; }
        public bool IsValid { get; set; }
    }
    private class Split
    {
        public byte SplitIndex { get; init; }
        public required Detection StartDetection { get; init; }
        public Detection? EndDetection { get; set; }
        public TimeSpan Duration { get; set; }
    }
    private class ConsecutiveRecord
    {
        public uint LapCount { get; init; }
        public TimeSpan Record { get; init; }
        public Guid[] IncludedLaps { get; init; } = [];
    }

    private Guid[] _trackDetectors = [];

    private Detection? _mostRecentDetection;
    private Lap? _currentLap;
    private Split? _currentSplit;

    private List<Lap> _completedLaps = [];

    private ConsecutiveRecord? _maxConsecutiveRecord;

    private readonly Dictionary<uint, ConsecutiveRecord> _bestConsecutiveLapRecords = new();

    private TimeSpan? _redetectionDelay;
    private TimeSpan? _maximumLapTime;

    public OpenPracticePilotTiming()
    {

    }
    public OpenPracticePilotTiming(Guid pilotId, Guid sessionId, Guid trackId, Guid[] trackDetectors, long? redetectionDelayMilliseconds, long? maximumLapMilliseconds)
    {
        Raise(new OpenPracticePilotTimingStarted(pilotId, sessionId, trackId, trackDetectors, redetectionDelayMilliseconds, maximumLapMilliseconds));
    }
    private static byte GetNextSplitIndex(byte previousIndex, byte totalSplits)
    {
        byte nextIndex = (byte)(previousIndex + 1);
        if (nextIndex >= totalSplits)
        {
            nextIndex = 0;
        }
        return nextIndex;
    }
    private static TimeSpan CalculateDuration(Guid? startTimerId, ulong? startHardwareTime, long startSoftwareTime, DateTime startUtcTime,
        Guid? endTimerId, ulong? endHardwareTime, long endSoftwareTime, DateTime endUtcTime)
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
    private static Dictionary<uint, ConsecutiveRecord> GetGroupRecords(List<Lap> lapGroup)
    {
        Dictionary<uint, ConsecutiveRecord> records = new();
        for (uint groupSize = 1; groupSize <= lapGroup.Count; groupSize++)
        {
            TimeSpan? bestDuration = null;
            Guid[] bestLapIds = Array.Empty<Guid>();

            // Slide a window of size groupSize over the validLaps
            for (int start = 0; start <= lapGroup.Count - groupSize; start++)
            {
                var group = lapGroup.Skip(start).Take((int)groupSize).ToList();
                TimeSpan totalDuration = group.Aggregate(TimeSpan.Zero, (sum, lap) => sum + lap.Duration!.Value);
                Guid[] lapIds = group.Select(lap => lap.Id).ToArray();

                if (bestDuration == null || totalDuration < bestDuration)
                {
                    bestDuration = totalDuration;
                    bestLapIds = lapIds;
                }
            }

            if (bestDuration != null)
            {
                records[groupSize] = new ConsecutiveRecord
                {
                    LapCount = groupSize,
                    Record = bestDuration.Value,
                    IncludedLaps = bestLapIds
                };
            }
        }

        return records;
    }
    public void AddDetectionOccurance(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        ArgumentNullException.ThrowIfNull(Id);

        int trackTimerOrdinal = Array.FindIndex(_trackDetectors, x => x == timerId);

        if (trackTimerOrdinal < 0 || trackTimerOrdinal >= _trackDetectors.Length)
        {
            Raise(new OpenPracticePilotDetectionOccuredOnInvalidTimer(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        if (_mostRecentDetection is not null && trackTimerOrdinal == _mostRecentDetection.TrackTimerOrdinal && _redetectionDelay.HasValue)
        {
            TimeSpan timeSinceLastDetection = CalculateDuration(_mostRecentDetection.TimerId, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime,
                timerId, hardwareTime, softwareTime, utcTime);
            if (timeSinceLastDetection < _redetectionDelay.Value)
            {
                Raise(new OpenPracticePilotDetectionOccuredToCloseToPreviousDetection(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, lane, hardwareTime, softwareTime, utcTime,
                    _mostRecentDetection.Id, _mostRecentDetection.TimerId, _mostRecentDetection.Lane, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime));
                return;
            }
        }

        if (_mostRecentDetection is not null && utcTime < _mostRecentDetection.UtcTime)
        {
            Raise(new OpenPracticePilotDetectionOccuredOutOfOrder(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, (byte)trackTimerOrdinal, (byte)_trackDetectors.Length, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        Raise(new OpenPracticePilotDetectionOccured(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, timerId, (byte)trackTimerOrdinal, (byte)_trackDetectors.Length, lane, hardwareTime, softwareTime, utcTime));
        RaiseLapEvents(detectionId, timerId, (byte)trackTimerOrdinal, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime);
        RaiseLapRecordEvents();
    }
    public void TriggerDetection(Guid detectionId, byte lane, byte ordinalPosition, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        ArgumentNullException.ThrowIfNull(Id);

        if (_mostRecentDetection is not null && ordinalPosition == _mostRecentDetection.TrackTimerOrdinal && _redetectionDelay.HasValue)
        {
            TimeSpan timeSinceLastDetection = CalculateDuration(_mostRecentDetection.TimerId, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime,
                null, hardwareTime, softwareTime, utcTime);
            if (timeSinceLastDetection < _redetectionDelay.Value)
            {
                Raise(new OpenPracticePilotDetectionTriggeredToCloseToPreviousDetection(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, lane, hardwareTime, softwareTime, utcTime,
                    _mostRecentDetection.Id, _mostRecentDetection.TimerId, _mostRecentDetection.Lane, _mostRecentDetection.HardwareTime, _mostRecentDetection.SoftwareTime, _mostRecentDetection.UtcTime));
                return;
            }
        }


        if (_mostRecentDetection is not null && utcTime < _mostRecentDetection.UtcTime)
        {
            Raise(new OpenPracticePilotDetectionTriggeredOutOfOrder(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, ordinalPosition, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime));
            return;
        }

        Raise(new OpenPracticePilotDetectionTriggered(detectionId, Id.SessionId, Id.TrackId, Id.PilotId, lane, ordinalPosition, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime));
        RaiseLapEvents(detectionId, null, ordinalPosition, (byte)_trackDetectors.Length, hardwareTime, softwareTime, utcTime);
    }

    private void RaiseLapEvents(Guid detectionId, Guid? timerId, byte trackTimerOrdinal, byte trackTimerTotal, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        ArgumentNullException.ThrowIfNull(Id);
        if (_currentSplit is not null && _currentLap is not null)
        {
            byte expectedNextSplitIndex = GetNextSplitIndex(_currentSplit.SplitIndex, trackTimerTotal);
            if (trackTimerOrdinal != expectedNextSplitIndex)
            {
                Raise(new OpenPracticeLapSplitEndMissed(Id.PilotId, Id.SessionId, Id.TrackId, _currentLap.Id, expectedNextSplitIndex, trackTimerOrdinal, detectionId, hardwareTime, softwareTime, utcTime));
                if (expectedNextSplitIndex == 0)
                {
                    Raise(new OpenPracticeLapEndMissed(Id.PilotId, Id.SessionId, Id.TrackId, _currentLap.Id, trackTimerOrdinal, detectionId, hardwareTime, softwareTime, utcTime));
                }
            }
            else
            {
                TimeSpan splitDuration = CalculateDuration(_currentSplit.StartDetection.TimerId, _currentSplit.StartDetection.HardwareTime, _currentSplit.StartDetection.SoftwareTime, _currentSplit.StartDetection.UtcTime,
                    timerId, hardwareTime, softwareTime, utcTime);

                Raise(new OpenPracticeLapSplitCompleted(Id.PilotId, Id.SessionId, Id.TrackId, _currentLap.Id, _currentSplit.SplitIndex,
                    _currentSplit.StartDetection.Id, _currentSplit.StartDetection.HardwareTime, _currentSplit.StartDetection.SoftwareTime, _currentSplit.StartDetection.UtcTime,
                    detectionId, hardwareTime, softwareTime, utcTime, splitDuration));

                if (trackTimerOrdinal == 0)
                {
                    TimeSpan lapDuration = CalculateDuration(_currentLap.StartDetection.TimerId, _currentLap.StartDetection.HardwareTime, _currentLap.StartDetection.SoftwareTime, _currentLap.StartDetection.UtcTime,
                        timerId, hardwareTime, softwareTime, utcTime);

                    Raise(new OpenPracticeLapCompleted(Id.PilotId, Id.SessionId, Id.TrackId, _currentLap.Id,
                        _currentLap.StartDetection.Id, _currentLap.StartDetection.HardwareTime, _currentLap.StartDetection.SoftwareTime, _currentLap.StartDetection.UtcTime,
                        detectionId, hardwareTime, softwareTime, utcTime, lapDuration));
                }
            }
        }

        Guid lapId = trackTimerOrdinal == 0 ? Guid.NewGuid() : _currentLap?.Id ?? Guid.NewGuid();

        Raise(new OpenPracticeLapSplitStarted(Id.PilotId, Id.SessionId, Id.TrackId, lapId, trackTimerOrdinal, detectionId, hardwareTime, softwareTime, utcTime));

        if (trackTimerOrdinal == 0)
        {
            Raise(new OpenPracticeLapStarted(Id.PilotId, Id.SessionId, Id.TrackId, lapId, detectionId, hardwareTime, softwareTime, utcTime));
        }
    }
    private void RaiseLapRecordEvents()
    {
        ArgumentNullException.ThrowIfNull(Id);
        List<List<Lap>> consecutiveLapGroups = GetConsecutiveLapGroups();
        Dictionary<uint, ConsecutiveRecord> records = new();
        ConsecutiveRecord? maxRecord = null;

        bool recordChanged = false;
        foreach (List<Lap> lapGroup in consecutiveLapGroups)
        {
            if (maxRecord is null || lapGroup.Count > maxRecord.LapCount)
            {
                maxRecord = new()
                {
                    LapCount = (uint)lapGroup.Count,
                    Record = lapGroup.Aggregate(TimeSpan.Zero, (total, lap) => total + (lap.Duration ?? TimeSpan.Zero)),
                    IncludedLaps = lapGroup.Select(l => l.Id).ToArray()
                };
            }

            Dictionary<uint, ConsecutiveRecord> groupRecords = GetGroupRecords(lapGroup);

            foreach (KeyValuePair<uint, ConsecutiveRecord> kvp in groupRecords)
            {
                if (!records.ContainsKey(kvp.Key) || kvp.Value.Record < records[kvp.Key].Record)
                {
                    records[kvp.Key] = kvp.Value;
                }
            }
        }

        foreach (KeyValuePair<uint, ConsecutiveRecord> kvp in records)
        {
            if (_bestConsecutiveLapRecords.TryGetValue(kvp.Key, out ConsecutiveRecord? existingRecord))
            {
                if (kvp.Key == 1)
                {
                    Raise(new OpenPracticeFastestLapRecordImproved(Id.PilotId, Id.SessionId, Id.TrackId, existingRecord.Record, kvp.Value.Record, existingRecord.IncludedLaps.First()));
                    recordChanged = true;
                }
                else
                {
                    Raise(new OpenPracticeFastestConsecutiveLapsRecordImproved(Id.PilotId, Id.SessionId, Id.TrackId, kvp.Key, kvp.Value.IncludedLaps, existingRecord.Record, kvp.Value.Record));
                    recordChanged = true;
                }
            }
            else
            {
                if (kvp.Key == 1)
                {
                    Raise(new OpenPracticeFastestLapRecordRecorded(Id.PilotId, Id.SessionId, Id.TrackId, kvp.Value.Record, kvp.Value.IncludedLaps.First()));
                    recordChanged = true;
                }
                else
                {
                    Raise(new OpenPracticeFastestConsecutiveLapsRecordRecorded(Id.PilotId, Id.SessionId, Id.TrackId, kvp.Key, kvp.Value.IncludedLaps, kvp.Value.Record));
                    recordChanged = true;
                }
            }
        }

        if (maxRecord is not null)
        {
            if (_maxConsecutiveRecord is null)
            {
                Raise(new OpenPracticeConsecutiveLapRecordRecorded(Id.PilotId, Id.SessionId, Id.TrackId, maxRecord.LapCount, maxRecord.IncludedLaps));
                recordChanged = true;
            }
            else
            {
                Raise(new OpenPracticeConsecutiveLapRecordImproved(Id.PilotId, Id.SessionId, Id.TrackId, _maxConsecutiveRecord.LapCount, maxRecord.LapCount, maxRecord.IncludedLaps));
                recordChanged = true;
            }
        }

        if (!recordChanged)
        {
            Raise(new OpenPracticeLapCompletedWithNoEffectOnRecord());
        }
    }

    private List<List<Lap>> GetConsecutiveLapGroups()
    {
        List<List<Lap>> groups = [];
        List<Lap> currentGroup = [];
        foreach (Lap lap in _completedLaps)
        {
            if (!lap.IsValid || !lap.Duration.HasValue || (lap.Duration > _maximumLapTime))
            {
                if (currentGroup.Count > 0)
                {
                    groups.Add(currentGroup);
                    currentGroup = [];
                }
                continue;
            }
            currentGroup.Add(lap);
        }
        if (currentGroup.Count > 0)
        {
            groups.Add(currentGroup);
        }
        return groups;
    }

    private void When(OpenPracticePilotDetectionOccured occured)
    {
        _mostRecentDetection = new()
        {
            Id = occured.DetectionId,
            TimerId = occured.TimerId,
            TrackTimerOrdinal = occured.TrackTimerOrdinal,
            TrackTimerTotal = occured.TrackTimerTotal,
            HardwareTime = occured.HardwareTime,
            SoftwareTime = occured.SoftwareTime,
            UtcTime = occured.UtcTime
        };
    }
    private void When(OpenPracticePilotTimingStarted started)
    {
        Id = new OpenPracticePilotTimingId
        {
            PilotId = started.PilotId,
            SessionId = started.SessionId,
            TrackId = started.TrackId
        };
        _redetectionDelay = started.RedetectionDelayMilliseconds.HasValue ? TimeSpan.FromMilliseconds(started.RedetectionDelayMilliseconds.Value) : null;
        _maximumLapTime = started.MaximumLapMilliseconds.HasValue ? TimeSpan.FromMilliseconds(started.MaximumLapMilliseconds.Value) : null;
        _trackDetectors = started.TrackDetectors;
    }
    private void When(OpenPracticePilotDetectionTriggered triggered)
    {
        _mostRecentDetection = new()
        {
            Id = triggered.DetectionId,
            TimerId = null,
            TrackTimerOrdinal = triggered.OrdinalPosition,
            TrackTimerTotal = triggered.TrackDetectorCount,
            HardwareTime = triggered.HardwareTime,
            SoftwareTime = triggered.SoftwareTime,
            UtcTime = triggered.UtcTime
        };
    }
    private void When(OpenPracticeLapStarted started)
    {
        _currentLap = new()
        {
            Id = started.LapId,
            StartDetection = new()
            {
                Id = started.StartDetectionId,
                TimerId = null,
                TrackTimerOrdinal = 0,
                TrackTimerTotal = (byte)_trackDetectors.Length,
                HardwareTime = started.StartedHardwareTime,
                SoftwareTime = started.StartedSoftwareTime,
                UtcTime = started.StartedUtcTime
            },
            EndDetection = null
        };
    }
    private void When(OpenPracticeLapSplitStarted started)
    {
        _currentSplit = new()
        {
            SplitIndex = started.SplitIndex,
            StartDetection = new()
            {
                Id = started.StartDetectionId,
                TimerId = null,
                TrackTimerOrdinal = started.SplitIndex,
                TrackTimerTotal = (byte)_trackDetectors.Length,
                HardwareTime = started.StartedHardwareTime,
                SoftwareTime = started.StartedSoftwareTime,
                UtcTime = started.StartedUtcTime
            },
            EndDetection = null
        };
    }
    private void When(OpenPracticeLapCompleted completed)
    {
        if (_currentLap is not null && _currentLap.Id == completed.LapId)
        {
            _currentLap.EndDetection = new()
            {
                Id = completed.EndDetectionId,
                TimerId = null,
                TrackTimerOrdinal = 0,
                TrackTimerTotal = (byte)_trackDetectors.Length,
                HardwareTime = completed.CompletedHardwareTime,
                SoftwareTime = completed.CompletedSoftwareTime,
                UtcTime = completed.CompletedUtcTime
            };
            _currentLap.IsValid = true;
            _currentLap.Duration = completed.Duration;
            _completedLaps.Add(_currentLap);
            _currentLap = null;
            _currentSplit = null;
        }
    }
    private void When(OpenPracticeLapSplitCompleted completed)
    {
        if (_currentSplit is not null && _currentLap is not null && _currentLap.Id == completed.LapId && _currentSplit.SplitIndex == completed.SplitIndex)
        {
            _currentSplit.EndDetection = new()
            {
                Id = completed.EndDetectionId,
                TimerId = null,
                TrackTimerOrdinal = completed.SplitIndex,
                TrackTimerTotal = (byte)_trackDetectors.Length,
                HardwareTime = completed.CompletedHardwareTime,
                SoftwareTime = completed.CompletedSoftwareTime,
                UtcTime = completed.CompletedUtcTime
            };
            _currentSplit.Duration = completed.Duration;
            _currentLap.Splits.Add(_currentSplit);
            _currentSplit = null;
        }
    }
    private void When(OpenPracticeFastestLapRecordRecorded recorded)
    {
        _bestConsecutiveLapRecords[1] = new()
        {
            LapCount = 1,
            Record = recorded.Record,
            IncludedLaps = [recorded.LapId]
        };
    }
    private void When(OpenPracticeFastestLapRecordImproved improved)
    {
        _bestConsecutiveLapRecords[1] = new()
        {
            LapCount = 1,
            Record = improved.NewRecord,
            IncludedLaps = [improved.LapId]
        };
    }
    private void When(OpenPracticeConsecutiveLapRecordRecorded recorded)
    {
        _maxConsecutiveRecord = new()
        {
            LapCount = recorded.LapCount,
            IncludedLaps = recorded.IncludedLaps
        };
    }
    private void When(OpenPracticeConsecutiveLapRecordImproved improved)
    {
        _maxConsecutiveRecord = new()
        {
            LapCount = improved.NewConsecutiveLapCount,
            IncludedLaps = improved.IncludedLaps
        };
    }
    private void When(OpenPracticeFastestConsecutiveLapsRecordRecorded recorded)
    {
        _bestConsecutiveLapRecords[recorded.LapCount] = new()
        {
            LapCount = recorded.LapCount,
            Record = recorded.Record,
            IncludedLaps = recorded.IncludedLaps
        };
    }
    private void When(OpenPracticeFastestConsecutiveLapsRecordImproved improved)
    {
        _bestConsecutiveLapRecords[improved.LapCount] = new()
        {
            LapCount = improved.LapCount,
            Record = improved.NewRecord,
            IncludedLaps = improved.IncludedLaps
        };
    }
}
