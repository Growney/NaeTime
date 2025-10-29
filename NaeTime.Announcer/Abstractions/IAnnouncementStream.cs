namespace NaeTime.Announcer.Abstractions;
public interface IAnnouncementStream : IDisposable
{
    public IAsyncEnumerable<string> StreamAnnouncments(CancellationToken cancellationToken = default);
}
