using Microsoft.Extensions.Hosting;
using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;

namespace NaeTime.Announcer;
public class Announcer : BackgroundService
{
    private readonly IEnumerable<IAnnouncmentProvider> _providers;
    private readonly ISpeechProvider _speechProvider;

    public Announcer(IEnumerable<IAnnouncmentProvider> providers, ISpeechProvider speechProvider)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _speechProvider = speechProvider ?? throw new ArgumentNullException(nameof(speechProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(250));

        while (!stoppingToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
            foreach (IAnnouncmentProvider provider in _providers)
            {
                //Doing it this way allows for the first provider to have priority, BUT could end up with a provider blocking the others constantly if they are constantly returning announcements may need to look at a better way to handle this.
                Announcement? nextAnnouncement;
                do
                {
                    nextAnnouncement = await provider.GetNextAnnouncement();
                    if (nextAnnouncement != null)
                    {
                        await Announce(nextAnnouncement);
                    }

                } while (nextAnnouncement != null);
            }
        }
    }

    private Task Announce(Announcement announcement) => _speechProvider.SpeakAsync(announcement.Message);

}
