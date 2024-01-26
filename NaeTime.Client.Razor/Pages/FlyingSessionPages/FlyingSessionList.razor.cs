using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Pages.FlyingSessionPages;
public partial class FlyingSessionList
{
    [Inject]
    private IFlyingSessionApiClient FlyingSessionApiClient { get; set; } = null!;
    [Inject]
    private ITrackApiClient TrackApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<FlyingSession> _flyingSessions = new();
    private readonly List<Track> _tracks = new();

    protected override async Task OnInitializedAsync()
    {
        var existingPilots = await FlyingSessionApiClient.GetAllAsync();

        _flyingSessions.AddRange(existingPilots);

        var existingTracks = await TrackApiClient.GetAllAsync();

        _tracks.AddRange(existingTracks);

        await base.OnInitializedAsync();
    }

    private void NavigateToFlyingSession(FlyingSession flyingSession)
    {
        NavigationManager.NavigateTo($"/flyingsession/update/{flyingSession.Id}");
    }
    private void NavigateToCreateFlyingSession()
    {
        NavigationManager.NavigateTo($"/flyingsession/create");
    }
}