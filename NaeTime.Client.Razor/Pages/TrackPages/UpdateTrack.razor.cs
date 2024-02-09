using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Messages.Events.Entities;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class UpdateTrack
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid TrackId { get; set; }
    [Parameter]
    public string? ReturnUrl { get; set; }

    private Track? _model;

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        var trackResponse = await Dispatcher.Request<TrackRequest, TrackResponse>(new(TrackId));

        if (trackResponse == null)
        {
            return;
        }
        _model = new Track()
        {
            Id = trackResponse.Id,
            Name = trackResponse.Name,
            MaximumLapTimeMilliseconds = trackResponse.MaximumLapTimeMilliseconds,
            MinimumLapTimeMilliseconds = trackResponse.MinimumLapTimeMilliseconds,
        };
        _model.AddTimers(trackResponse.Timers);

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

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Track track)
    {
        await Dispatcher.Dispatch(new TrackDetailsChanged(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers));

        var returnUrl = ReturnUrl ?? "/track/list";

        NavigationManager.NavigateTo(returnUrl);
    }
}
