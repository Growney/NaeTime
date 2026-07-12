using NaeTime.Announcer.Abstractions;
using NaeTime.Collections;
using NaeTime.Events.Domain;
using NaeTime.Hardware.Frequency;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Announcer;
public class AnnouncerReactions : IAnnouncementStream
{

    private static readonly uint[] _announcedLapRecords = { 1 };
    private readonly AwaitableQueue<string> _announcementQueue = new(100);

    private readonly ITrackQueryHandler _trackProjection;
    private readonly IPilotQueryHandler _pilotProjection;
    private readonly IOpenPracticeQueryHandler _openPracticeProjection;
    private readonly IHardwareQueryHandler _hardwareQueryHandler;

    public AnnouncerReactions(ITrackQueryHandler trackProjection, IPilotQueryHandler pilotProjection, IOpenPracticeQueryHandler openPracticeProjection, IHardwareQueryHandler hardwareQueryHandler)
    {
        _trackProjection = trackProjection ?? throw new ArgumentNullException(nameof(trackProjection));
        _pilotProjection = pilotProjection ?? throw new ArgumentNullException(nameof(pilotProjection));
        _openPracticeProjection = openPracticeProjection ?? throw new ArgumentNullException(nameof(openPracticeProjection));
        _hardwareQueryHandler = hardwareQueryHandler ?? throw new ArgumentNullException(nameof(hardwareQueryHandler));
    }

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
                Console.WriteLine($"Announcement: {announcement}");
                yield return announcement;
            }
        }
    }
    private async Task HandleDetection(Guid pilotId, Guid detectionId, Guid sessionId, Guid trackId)
    {
        string? callout = await GetPilotCallout(pilotId);
        if (string.IsNullOrEmpty(callout))
        {
            return;
        }

        SessionTimingInformation? timingInfo = await _openPracticeProjection.GetTimingInformation(sessionId, trackId);

        if (timingInfo is null)
        {
            return;
        }

        if (!timingInfo.PilotLapGroups.TryGetValue(pilotId, out IEnumerable<IEnumerable<OpenPracticeLap>>? lapGroups))
        {
            return;
        }

        OpenPracticeLap? detectionFinalisedLap = lapGroups.SelectMany(x => x).Where(lap => lap.EndDetection.Id == detectionId).FirstOrDefault();

        if (detectionFinalisedLap is null)
        {
            _announcementQueue.Enqueue(callout);
            return;
        }

        IEnumerable<OpenPracticeLapRecord> includedInLapRecords = Enumerable.Empty<OpenPracticeLapRecord>();
        if (timingInfo.PilotLapRecords.TryGetValue(pilotId, out IDictionary<uint, OpenPracticeLapRecord>? pilotLapRecords))
        {
            includedInLapRecords = pilotLapRecords.Values.Where(record => _announcedLapRecords.Contains(record.LapCount) && record.IncludedLaps.Last().EndDetection.Id == detectionId);
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
    private Task When(OpenPracticePilotDetectionOccured occured) => HandleDetection(occured.PilotId, occured.DetectionId, occured.SessionId, occured.TrackId);

    private async Task<string?> GetPilotCallout(Guid pilotId)
    {
        Pilot? pilot = await _pilotProjection.GetPilotById(pilotId);
        return pilot?.Callsign ?? pilot?.Firstname ?? pilot?.Lastname;
    }
    private Task When(OpenPracticePilotDetectionTriggered triggered) => HandleDetection(triggered.PilotId, triggered.DetectionId, triggered.SessionId, triggered.TrackId);

    private static string GetLapCallout(TimeSpan timeSpan, int roundedTo = 3) => Math.Round(timeSpan.TotalSeconds, roundedTo).ToString("#.0" + String.Join("",Enumerable.Repeat("#", Math.Max(roundedTo-1,0))));

    private async Task AnnouncePilotFrequency(Guid sessionId, Guid? pilotId, byte laneId)
    {
        OpenPracticeSession? session = await _openPracticeProjection.GetByIdAsync(sessionId);
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

        string? callout = await GetPilotCallout(pilotId.Value);
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
    private Task When(OpenPracticeSessionLanePilotSet pilotSet) => AnnouncePilotFrequency(pilotSet.SessionId, pilotSet.PilotId, pilotSet.Lane);
    private Task When(OpenPracticeSessionLaneVideoFrequencyTuned frequencyTuned) => AnnouncePilotFrequency(frequencyTuned.SessionId, null, frequencyTuned.Lane);

    private async Task When(TimerConnected connected)
    {
        Detector? detector = await _hardwareQueryHandler.GetDetector(connected.TimerId);

        if (detector is null)
        {
            return;
        }

        _announcementQueue.Enqueue($"{detector.Name} connected");
    }

    private async Task When(TimerDisconnected disconnected)
    {
        Detector? detector = await _hardwareQueryHandler.GetDetector(disconnected.TimerId);

        if (detector is null)
        {
            return;
        }

        _announcementQueue.Enqueue($"{detector.Name} disconnected");
    }
}
