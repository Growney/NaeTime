using NaeTime.Announcer.Models;

namespace NaeTime.Announcer.Abstractions;
public interface IAnnouncmentProvider
{
    Task<Announcement?> GetNextAnnouncement();
}
