using NaeTime.Announcer.Abstractions;
using NaeTime.Collections;
using System.Runtime.CompilerServices;

namespace NaeTime.Announcer;
public class AnnouncerReactions : IAnnouncementStream
{
    private readonly AwaitableQueue<string> _announcementQueue = new(100);

    public void Dispose()
    {
        _announcementQueue.Dispose();
    }

    public async IAsyncEnumerable<string> StreamAnnouncments([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            string? announcement = await _announcementQueue.WaitForDequeueAsync(cancellationToken);
            if (announcement != null)
            {
                yield return announcement;
            }
        }
    }
}
