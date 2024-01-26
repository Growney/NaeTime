using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class UpdateTrack
{
    [Inject]
    private ITrackApiClient TrackApiClient { get; set; } = null!;
    [Inject]
    private IHardwareApiClient HardwareApiClient { get; set; } = null!;
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
        var timers = await HardwareApiClient.GetAllTimerDetailsAsync();
        _timers.AddRange(timers);

        _model = await TrackApiClient.GetAsync(TrackId);

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Track track)
    {
        await TrackApiClient.UpdateAsync(track);

        var returnUrl = ReturnUrl ?? "/track/list";

        NavigationManager.NavigateTo(returnUrl);
    }
}
