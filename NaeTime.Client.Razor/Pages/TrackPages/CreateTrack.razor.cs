using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class CreateTrack
{
    [Inject]
    private ITrackApiClient TrackApiClient { get; set; } = null!;
    [Inject]
    private IHardwareApiClient HardwareApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly Track _model = new(Guid.NewGuid(), string.Empty, Enumerable.Empty<Guid>());

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        var timers = await HardwareApiClient.GetAllTimerDetailsAsync();

        _timers.AddRange(timers);

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Track track)
    {
        await TrackApiClient.CreateAsync(track.Name, track.TimedGates);

        NavigationManager.NavigateTo(ReturnUrl ?? "/track/list");
    }
}