using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Messages.Events.Entities;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class CreateTrack
{

    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
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
        var timersResponse = await Dispatcher.Request<TimerDetailsRequest, TimerDetailsResponse>();

        if (timersResponse == null)
        {
            return;
        }


        _timers.AddRange(timersResponse.Timers.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                TimerDetailsResponse.TimerType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                _ => throw new NotImplementedException()
            })));

    }

    private async Task HandleValidSubmit(Track track)
    {
        await Dispatcher.Dispatch(new TrackCreated(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers));

        NavigationManager.NavigateTo(ReturnUrl ?? "/track/list");
    }
}