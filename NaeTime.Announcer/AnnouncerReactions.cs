using NaeTime.Announcer.Abstractions;
using NaeTime.Collections;
using NaeTime.Events;

namespace NaeTime.Announcer;
public class AnnouncerReactions : IAnnouncementStream
{
    private readonly AwaitableQueue<string> _announcementQueue = new(100);

    public void Dispose()
    {
        _announcementQueue.Dispose();
    }

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
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

    private void When(OpenPracticePilotDetectionOccured occured)
    {
        string announcement = "Detected";

        _announcementQueue.Enqueue(announcement);
    }
    private void When(OpenPracticePilotDetectionTriggered triggered)
    {
        string announcement = "Triggered";
        _announcementQueue.Enqueue(announcement);
    }
}
