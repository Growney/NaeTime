using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class TrackList
{
    [Inject]
    private ITrackApiClient TrackApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Track> _tracks = new();

    protected override async Task OnInitializedAsync()
    {
        var existingPilots = await TrackApiClient.GetAllAsync();

        _tracks.AddRange(existingPilots);

        await base.OnInitializedAsync();
    }

    private void NavigateToTrack(Track track)
    {
        NavigationManager.NavigateTo($"/track/update/{track.Id}");
    }
    private void NavigateToCreateTrack()
    {
        NavigationManager.NavigateTo($"/track/create");
    }
}