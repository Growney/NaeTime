using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;
using NaeTime.Hardware.Messages;

namespace NaeTime.Announcer;
public class HardwareAnnouncerService : IAnnouncmentProvider
{
    private Announcement? _nextAnnouncement;

    public void When(TimerConnectionEstablished timerConnected) => _nextAnnouncement = new Models.Announcement("Timer connected");
    public void When(TimerDisconnected timerDisconnected) => _nextAnnouncement = new Models.Announcement("Timer disconnected");
    public Task<Announcement?> GetNextAnnouncement()
    {
        Announcement? announcement = _nextAnnouncement;
        _nextAnnouncement = null;
        return Task.FromResult(announcement);
    }
}