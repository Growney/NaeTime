using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;
using System.Collections.Concurrent;

namespace NaeTime.Announcer;
public class AnnouncementQueue : IAnnouncmentQueue
{
    private readonly ConcurrentQueue<Announcement> _queue = new();

    public void Enqueue(Announcement announcement) => _queue.Enqueue(announcement);

    public bool TryDequeue(out Announcement? announcement) => _queue.TryDequeue(out announcement);
}
