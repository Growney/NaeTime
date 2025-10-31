using NaeTime.Announcer.Abstractions;
using NaeTime.Collections;
using NaeTime.Events;
using NaeTime.Hardware.Frequency;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Announcer;
public class AnnouncerReactions : IAnnouncementStream
{
    private readonly AwaitableQueue<string> _announcementQueue = new(100);

    private readonly IOpenPracticeTimingProjection _openPracticeTimingProjection;
    private readonly ITrackProjection _trackProjection;
    private readonly IPilotProjection _pilotProjection;
    private readonly IOpenPracticeProjection _openPracticeProjection;
    private readonly IDetectorProjection _detectorProjection;

    public AnnouncerReactions(IOpenPracticeTimingProjection openPracticeTimingProjection, ITrackProjection trackProjection, IPilotProjection pilotProjection, IOpenPracticeProjection openPracticeProjection, IDetectorProjection detectorProjection)
    {
        _openPracticeTimingProjection = openPracticeTimingProjection ?? throw new ArgumentNullException(nameof(openPracticeTimingProjection));
        _trackProjection = trackProjection ?? throw new ArgumentNullException(nameof(trackProjection));
        _pilotProjection = pilotProjection ?? throw new ArgumentNullException(nameof(pilotProjection));
        _openPracticeProjection = openPracticeProjection ?? throw new ArgumentNullException(nameof(openPracticeProjection));
        _detectorProjection = detectorProjection ?? throw new ArgumentNullException(nameof(detectorProjection));
    }

    private IEnumerable<Type> GetProjectionDependencies() => [typeof(IOpenPracticeTimingProjection), typeof(ITrackProjection), typeof(IPilotProjection)];

    public void Dispose()
    {
        _announcementQueue.Dispose();
    }

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            string? announcement = await _announcementQueue.WaitForDequeueAsync(cancellationToken);
            if (announcement != null)
            {
                yield return announcement;
            }
        }
    }

    private void When(OpenPracticePilotDetectionOccured occured)
    {
        string announcement = "Detected";

        _announcementQueue.Enqueue(announcement);
    }

    private string? GetPilotCallout(Guid pilotId)
    {
        Pilot? pilot = _pilotProjection.GetPilotById(pilotId);
        return pilot?.Callsign ?? pilot?.Firstname ?? pilot?.Lastname;
    }
    private async Task When(OpenPracticePilotDetectionTriggered triggered)
    {
        string? callout = GetPilotCallout(triggered.PilotId);
        if (string.IsNullOrEmpty(callout))
        {
            return;
        }

        Track? track = await _trackProjection.GetTrack(triggered.TrackId);
        if (track is null)
        {
            return;
        }

        OpenPracticeSessionTimingInformation? timingInfo = _openPracticeTimingProjection.GetSessionTimingInfo(triggered.SessionId, triggered.TrackId,
            track.MinimumLapTimeMilliseconds.HasValue ? TimeSpan.FromMilliseconds(track.MinimumLapTimeMilliseconds.Value) : TimeSpan.Zero,
            track.MaximumLapTimeMilliseconds.HasValue ? TimeSpan.FromMilliseconds(track.MaximumLapTimeMilliseconds.Value) : TimeSpan.MaxValue);

        if (timingInfo is null)
        {
            return;
        }

        if (!timingInfo.PilotLapGroups.TryGetValue(triggered.PilotId, out IEnumerable<IEnumerable<OpenPracticeLap>>? lapGroups))
        {
            return;
        }

        OpenPracticeLap? detectionFinalisedLap = lapGroups.SelectMany(x => x).Where(lap => lap.EndDetection.Id == triggered.DetectionId).FirstOrDefault();

        if (detectionFinalisedLap is null)
        {
            _announcementQueue.Enqueue(callout);
            return;
        }

        IEnumerable<OpenPracticeLapRecord> includedInLapRecords = Enumerable.Empty<OpenPracticeLapRecord>();
        if (timingInfo.PilotLapRecords.TryGetValue(triggered.PilotId, out IDictionary<uint, OpenPracticeLapRecord>? pilotLapRecords))
        {
            includedInLapRecords = pilotLapRecords.Values.Where(record => record.IncludedLaps.Any(lap => lap.EndDetection.Id == triggered.DetectionId));
        }

        if (!includedInLapRecords.Any())
        {
            _announcementQueue.Enqueue($"{callout} {GetLapCallout(detectionFinalisedLap.Duration)}");
            return;
        }

        uint highestLapCount = includedInLapRecords.Max(record => record.LapCount);
        OpenPracticeLapRecord record = includedInLapRecords.First(x => x.LapCount == highestLapCount);

        IEnumerable<OpenPracticeLapRecord> sessionLapRecords = timingInfo.SessionLapRecords[highestLapCount];
        bool isBestOverall = record.Record == sessionLapRecords.Min(r => r.Record);

        if (highestLapCount == 1)
        {
            if (isBestOverall)
            {
                _announcementQueue.Enqueue($"Single Lap Record, {callout}, {GetLapCallout(record.Record)}");
            }
            else
            {
                _announcementQueue.Enqueue($"Single Lap PB, {callout}, {GetLapCallout(record.Record)}");
            }
        }
        else
        {
            if (isBestOverall)
            {
                _announcementQueue.Enqueue($"{highestLapCount} lap record, {callout}, {GetLapCallout(record.Record)}");
            }
            else
            {
                _announcementQueue.Enqueue($"{highestLapCount} Lap PB, {callout}, {GetLapCallout(record.Record)}");
            }
        }
    }

    private static string GetLapCallout(TimeSpan timeSpan, int roundedTo = 3) => Math.Round(timeSpan.TotalSeconds, roundedTo).ToString();

    private void AnnouncePilotFrequency(Guid sessionId, Guid? pilotId, byte laneId)
    {
        OpenPracticeSession? session = _openPracticeProjection.GetSession(sessionId);
        if (session is null)
        {
            return;
        }

        OpenPracticeLane? lane = session.Lanes.FirstOrDefault(l => l.Lane == laneId);
        if (lane is null)
        {
            return;
        }

        pilotId ??= lane.PilotId;

        if (!pilotId.HasValue)
        {
            return;
        }

        string? callout = GetPilotCallout(pilotId.Value);
        if (string.IsNullOrEmpty(callout))
        {
            return;
        }

        Band band = Band.Bands.FirstOrDefault(b => b.Id == lane.BandId);

        BandFrequency? frequency = band.Frequencies.FirstOrDefault(f => f.FrequencyInMhz == lane.FrequencyInMHz);

        if (!frequency.HasValue)
        {
            _announcementQueue.Enqueue($"{callout} on {lane.FrequencyInMHz} MHz");
        }
        else
        {
            _announcementQueue.Enqueue($"{callout} on {frequency.Value.Name}");
        }
    }
    private void When(OpenPracticeSessionLanePilotSet pilotSet) => AnnouncePilotFrequency(pilotSet.SessionId, pilotSet.PilotId, pilotSet.Lane);
    private void When(OpenPracticeSessionLaneVideoFrequencyTuned frequencyTuned) => AnnouncePilotFrequency(frequencyTuned.SessionId, null, frequencyTuned.Lane);

    private void When(TimerConnected connected)
    {
        Detector? detector = _detectorProjection.GetDetector(connected.TimerId);

        if (detector is null)
        {
            return;
        }

        _announcementQueue.Enqueue($"{detector.Name} connected");
    }

    private void When(TimerDisconnected disconnected)
    {
        Detector? detector = _detectorProjection.GetDetector(disconnected.TimerId);

        if (detector is null)
        {
            return;
        }

        _announcementQueue.Enqueue($"{detector.Name} disconnected");
    }
}
