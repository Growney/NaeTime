using NaeTime.Announcer.Abstractions;
using NaeTime.Collections;
using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Announcer;
public class AnnouncerReactions : IAnnouncementStream
{
    private readonly AwaitableQueue<string> _announcementQueue = new(100);

    private readonly IOpenPracticeTimingProjection _openPracticeTimingProjection;
    private readonly ITrackProjection _trackProjection;
    private readonly IPilotProjection _pilotProjection;

    public AnnouncerReactions(IOpenPracticeTimingProjection openPracticeTimingProjection, ITrackProjection trackProjection, IPilotProjection pilotProjection)
    {
        _openPracticeTimingProjection = openPracticeTimingProjection ?? throw new ArgumentNullException(nameof(openPracticeTimingProjection));
        _trackProjection = trackProjection ?? throw new ArgumentNullException(nameof(trackProjection));
        _pilotProjection = pilotProjection ?? throw new ArgumentNullException(nameof(pilotProjection));
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
    private async Task When(OpenPracticePilotDetectionTriggered triggered)
    {
        Pilot? pilot = _pilotProjection.GetPilotById(triggered.PilotId);
        string? callout = pilot?.Callsign ?? pilot?.Firstname ?? pilot?.Lastname;
        if (pilot == null || string.IsNullOrEmpty(callout))
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

}
