using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Messages.Events.Timing;
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

    [Inject]
    public IDispatcher Dispatch { get; set; } = null!;

    private bool showRemove = false;

    public async Task Remove(Guid lapId, Guid pilotId)
    {
        await Dispatch.Dispatch(new OpenPracticeLapRemoved(SessionId, lapId, pilotId));
    }

}
