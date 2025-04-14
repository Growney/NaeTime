using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class CreateTrack
{
    [Inject]
    private INaeTimePersistence Persistence { get; set; } = null!;
    [Inject]
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;
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
        IEnumerable<NaeTime.Persistence.Abstractions.Hardware.TimerDetails> timersResponse = await Orchestrator.Hardware.GetAllTimerDetails();

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

    }

    private async Task HandleValidSubmit(Track track)
    {
        await Orchestrator.Management.CreateTrack(track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers);
        await Orchestrator.CommitAsync();

        NavigationManager.NavigateTo(ReturnUrl ?? "/track/list");
    }
}