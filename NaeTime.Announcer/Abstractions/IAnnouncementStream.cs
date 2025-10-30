namespace NaeTime.Announcer.Abstractions;
public interface IAnnouncementStream : IAsyncEnumerable<string>, IDisposable
{
}
