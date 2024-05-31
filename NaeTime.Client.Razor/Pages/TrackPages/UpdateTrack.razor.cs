using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Management.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class UpdateTrack
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventClient EventClient { get; set; } = null!;
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
        Management.Messages.Models.Track? trackResponse = await RpcClient.InvokeAsync<Management.Messages.Models.Track>("GetTrack", TrackId);

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

        IEnumerable<Hardware.Messages.Models.TimerDetails>? timersResponse = await RpcClient.InvokeAsync<IEnumerable<Hardware.Messages.Models.TimerDetails>>("GetAllTimerDetails");

        if (timersResponse == null)
        {
            return;
        }

        byte maxLanes = timersResponse.Max(x => x.MaxLanes);

        _timers.AddRange(timersResponse.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                Hardware.Messages.Models.TimerType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                _ => throw new NotImplementedException()
            }, maxLanes)));

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Track track)
    {
        byte maxLanes = _timers.Where(x => track.Timers.Contains(x.Id)).Max(x => x.MaxLanes);

        await EventClient.PublishAsync(new TrackDetailsChanged(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers, maxLanes));

        string returnUrl = ReturnUrl ?? "/track/list";

        NavigationManager.NavigateTo(returnUrl);
    }
}
