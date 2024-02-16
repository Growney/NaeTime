using Microsoft.Extensions.Hosting;
using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;

namespace NaeTime.Announcer;
public class Announcer : BackgroundService
{
    private readonly IAnnouncmentQueue _queue;
    private readonly ISpeechProvider _speechProvider;

    public Announcer(IAnnouncmentQueue queue, ISpeechProvider speechProvider)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _speechProvider = speechProvider ?? throw new ArgumentNullException(nameof(speechProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));

        while (!stoppingToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
            if (_queue.TryDequeue(out var announcement))
            {
                if (announcement is not null)
                {
                    await Announce(announcement);
                }
            }
        }
    }

    private Task Announce(Announcement announcement) => _speechProvider.SpeakAsync(announcement.Message);

}
