using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;
using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Announcer;
public class LapAnnouncerService : ISubscriber
{
    private readonly IAnnouncmentQueue _queue;
    private readonly IDispatcher _dispatcher;
    public LapAnnouncerService(IAnnouncmentQueue queue, IDispatcher dispatcher)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task When(OpenPracticeLapCompleted lapCompleted)
    {
        var pilot = await _dispatcher.Request<PilotRequest, PilotResponse>(new PilotRequest(lapCompleted.PilotId));

        if (pilot == null)
        {
            return;
        }

        var identifier = pilot.CallSign ?? pilot.FirstName ?? pilot.LastName;
        var lapTime = TimeSpan.FromMilliseconds(lapCompleted.TotalMilliseconds);

        var announcementText = $"{identifier} {lapTime.TotalSeconds}";

        _queue.Enqueue(new Announcement(announcementText));
    }
}
