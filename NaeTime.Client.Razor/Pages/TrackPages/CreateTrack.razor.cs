using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Management.Messages.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class CreateTrack
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventClient EventClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly Track _model = new()
    {
        Id = Guid.NewGuid(),
        Name = null
    };

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        IEnumerable<Hardware.Messages.Models.TimerDetails>? timersResponse = await RpcClient.InvokeAsync<IEnumerable<Hardware.Messages.Models.TimerDetails>>("GetAllTimerDetails");

        if (timersResponse == null)
        {
            return;
        }

        _timers.AddRange(timersResponse.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                Hardware.Messages.Models.TimerType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                _ => throw new NotImplementedException()
            }, x.MaxLanes)));

    }

    private async Task HandleValidSubmit(Track track)
    {
        byte maxLanes = _timers.Where(x => track.Timers.Contains(x.Id)).Max(x => x.MaxLanes);

        await EventClient.Publish(new TrackCreated(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers, maxLanes));

        NavigationManager.NavigateTo(ReturnUrl ?? "/track/list");
    }
}