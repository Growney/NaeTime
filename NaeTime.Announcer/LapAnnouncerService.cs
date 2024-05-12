using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Announcer;
public class LapAnnouncerService
{
    private readonly IAnnouncmentQueue _queue;
    private readonly IRemoteProcedureCallClient _rpcClient;
    public LapAnnouncerService(IAnnouncmentQueue queue, IRemoteProcedureCallClient rpcClient)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(OpenPracticeLapCompleted lapCompleted)
    {
        var pilot = await _rpcClient.InvokeAsync<Management.Messages.Models.Pilot>("GetPilot", lapCompleted.PilotId);

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
