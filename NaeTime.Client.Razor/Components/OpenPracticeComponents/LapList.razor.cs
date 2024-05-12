using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class LapList : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public IEnumerable<OpenPracticeLap> Laps { get; set; } = Enumerable.Empty<OpenPracticeLap>();

    [Parameter]
    public bool IncludePilot { get; set; }
    [Parameter]
    public Guid SessionId { get; set; }
    [Parameter]
    public string? Header { get; set; }

    [Inject]
    private IEventClient EventClient { get; set; } = null!;

    public Task Remove(Guid lapId, Guid pilotId) => EventClient.Publish(new OpenPracticeLapRemoved(SessionId, lapId, pilotId));
    public Task Invalidate(Guid lapId, Guid pilotId)
    {

        var lap = Laps.FirstOrDefault(l => l.Id == lapId);

        if (lap == null)
        {
            return Task.CompletedTask;
        }

        lap.Status = OpenPracticeLapStatus.Invalid;

        return EventClient.Publish(new OpenPracticeLapDisputed(SessionId, lapId, pilotId, OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid));
    }
    public Task Validate(Guid lapId, Guid pilotId)
    {
        var lap = Laps.FirstOrDefault(l => l.Id == lapId);

        if (lap == null)
        {
            return Task.CompletedTask;
        }

        lap.Status = OpenPracticeLapStatus.Completed;

        return EventClient.Publish(new OpenPracticeLapDisputed(SessionId, lapId, pilotId, OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed));
    }
}
