using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;

namespace NaeTime.Announcer;
public class HardwareAnnouncerService : IAnnouncmentProvider
{
    private Announcement? _nextAnnouncement;

    public Task<Announcement?> GetNextAnnouncement()
    {
        Announcement? announcement = _nextAnnouncement;
        _nextAnnouncement = null;
        return Task.FromResult(announcement);
    }
}