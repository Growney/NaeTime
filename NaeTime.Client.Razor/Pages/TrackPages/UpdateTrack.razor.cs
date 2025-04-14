using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class UpdateTrack
{
    [Inject]
    private INaeTimePersistence Persistence { get; set; } = null!;
    [Inject]
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;
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
        Persistence.Abstractions.Management.Track? trackResponse = await Orchestrator.Management.GetTrack(TrackId);

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

        IEnumerable<Persistence.Abstractions.Hardware.TimerDetails>? timersResponse = await Orchestrator.Hardware.GetAllTimerDetails();

        if (timersResponse == null)
        {
            return;
        }

        _timers.AddRange(timersResponse.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                NaeTime.Persistence.Abstractions.Hardware.TimerType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                NaeTime.Persistence.Abstractions.Hardware.TimerType.SerialEsp32Node => TimerType.SerialEsp32Node,
                _ => throw new NotImplementedException()
            })));

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Track track)
    {
        await Orchestrator.Management.UpdateTrack(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers);
        await Orchestrator.CommitAsync();

        string returnUrl = ReturnUrl ?? "/track/list";

        NavigationManager.NavigateTo(returnUrl);
    }
}
