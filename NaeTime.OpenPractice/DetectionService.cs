using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Messages;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
internal class DetectionService
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;
    private readonly ISoftwareTimer _softwareTimer;

    public DetectionService(IEventClient eventClient, IRemoteProcedureCallClient rpcClient, ISoftwareTimer softwareTimer)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public async Task When(TimerDetectionOccured detection)
    {
        Management.Messages.Models.ActiveSession? activeSessionReponse = await _rpcClient.InvokeAsync<Management.Messages.Models.ActiveSession?>("GetActiveSession");

        if (activeSessionReponse == null || activeSessionReponse.Type != Management.Messages.Models.ActiveSession.SessionType.OpenPractice)
        {
            return;
        }

        OpenPractice.Messages.Models.OpenPracticeSession? session = await _rpcClient.InvokeAsync<OpenPractice.Messages.Models.OpenPracticeSession?>("GetOpenPracticeSession", activeSessionReponse.SessionId);

        if (session == null)
        {
            return;
        }

        Management.Messages.Models.Track? track = await _rpcClient.InvokeAsync<Management.Messages.Models.Track?>("GetTrack", session.TrackId);

        if (track == null)
        {
            return;
        }

        int timerIndex = track.Timers.IndexOf(detection.TimerId);

        if (timerIndex == -1)
        {
            return;
        }

        OpenPractice.Messages.Models.PilotLane? lane = session.ActiveLanes.FirstOrDefault(l => l.Lane == detection.Lane);

        await _eventClient.PublishAsync(new OpenPractice.Messages.Events.OpenPracticeDetectionAdded(Guid.NewGuid(), session.Id, detection.TimerId, lane?.PilotId, (byte)timerIndex, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
    }

    public async Task When(OpenPracticeDetectionAdded detection)
    {
        IEnumerable<Messages.Models.OpenPracticeLaneDetection>? pilotLaneDetections = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.OpenPracticeLaneDetection>>("GetOpenPracticeLaneDetections", detection.SessionId, detection.TimerId, detection.Lane);

        if (pilotLaneDetections == null)
        {
            return;
        }

        List<Messages.Models.OpenPracticeLaneDetection> pilotLaneDetectionsList = new();

        foreach (Messages.Models.OpenPracticeLaneDetection pilotLaneDetection in pilotLaneDetections)
        {
            if (pilotLaneDetection.Id == detection.Id)
            {
                return;
            }

            pilotLaneDetectionsList.Add(pilotLaneDetection);
        }

        pilotLaneDetectionsList.Sort((x, y) => x.UtcTime.CompareTo(y.UtcTime));
    }
}
