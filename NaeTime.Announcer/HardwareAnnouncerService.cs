using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;

namespace NaeTime.Announcer;
public class HardwareAnnouncerService : IAnnouncmentProvider, IDisposable
{
    private Announcement? _nextAnnouncement;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public HardwareAnnouncerService(IDistributionReceiver receiver)
    {
        _ = receiver.Process<TimerDisconnected>(_cancellationTokenSource.Token, When);
        _ = receiver.Process<TimerConnected>(_cancellationTokenSource.Token, When);
    }

    public Task When(TimerConnected timerConnected)
    {
        _nextAnnouncement = new Announcement("Timer connected");
        return Task.CompletedTask;
    }
    public Task When(TimerDisconnected timerDisconnected)
    {
        _nextAnnouncement = new Announcement("Timer disconnected");
        return Task.CompletedTask;
    }

    public Task<Announcement?> GetNextAnnouncement()
    {
        Announcement? announcement = _nextAnnouncement;
        _nextAnnouncement = null;
        return Task.FromResult(announcement);
    }

    public void Dispose() => _cancellationTokenSource.Cancel();
}