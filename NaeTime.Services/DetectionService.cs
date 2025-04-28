using Microsoft.Extensions.Hosting;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Management;
using NaeTime.Persistence.Abstractions.Management;
using NaeTime.Persistence.Abstractions.OpenPractice;

namespace NaeTime.Services;
public class DetectionService : IHostedService
{
    private readonly INaeTimeOrchestrator _orchestrator;
    private readonly IDistributionReceiver _receiver;
    private readonly List<Task> _receivers = new();

    private ActiveSession? _activeSession;
    private CancellationTokenSource? _cancellationTokenSource;

    public DetectionService(INaeTimeOrchestrator orchestrator, IDistributionReceiver receiver)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _activeSession = await _orchestrator.Management.GetActiveSession();

        _receivers.Add(_receiver.Process<TimerDetectionOccured>(_cancellationTokenSource.Token, When));
        _receivers.Add(_receiver.Process<SessionDeactivated>(_cancellationTokenSource.Token, When));
        _receivers.Add(_receiver.Process<SessionActivated>(_cancellationTokenSource.Token, When));
    }
    private Task When(SessionDeactivated session)
    {
        if (_activeSession?.SessionId == session.SessionId)
        {
            _activeSession = null;
        }
        return Task.CompletedTask;
    }
    private Task When(SessionActivated session)
    {
        _activeSession = new ActiveSession(session.SessionId, session.Type);

        return Task.CompletedTask;
    }
    private Task When(TimerDetectionOccured detection)
    {
        if (_activeSession == null)
        {
            return Task.CompletedTask;
        }

        return _activeSession.Type switch
        {
            ActiveSession.SessionType.OpenPractice => HandleOpenPracticeDetection(_activeSession.SessionId, detection),
            _ => Task.CompletedTask,
        };
    }

    private async Task HandleOpenPracticeDetection(Guid sessionId, TimerDetectionOccured detection)
    {
        OpenPracticeSession? session = await _orchestrator.OpenPractice.GetOpenPracticeSession(sessionId);
        if (session == null)
        {
            return;
        }

        int timerIndex = await _orchestrator.Management.GetTrackTimerIndex(session.TrackId, detection.TimerId);

        if (timerIndex < 0)
        {
            return;
        }

        OpenPracticeLaneConfiguration laneConfiguration = await _orchestrator.OpenPractice.GetLaneConfiguration(sessionId, detection.Lane);

        await _orchestrator.Timing.AddSessionDetection(session.Id, session.TrackId, detection.TimerId, laneConfiguration.PilotId, timerIndex, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
        await _orchestrator.CommitAsync();
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        await Task.WhenAll(_receivers);
    }
}
