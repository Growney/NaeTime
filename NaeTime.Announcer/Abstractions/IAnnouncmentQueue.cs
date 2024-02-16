using NaeTime.Announcer.Models;

namespace NaeTime.Announcer.Abstractions;
public interface IAnnouncmentQueue
{
    void Enqueue(Announcement announcement);
    bool TryDequeue(out Announcement? announcement);
}
