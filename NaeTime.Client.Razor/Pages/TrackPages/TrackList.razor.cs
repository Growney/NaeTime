using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class TrackList
{
    [Inject]
    private ITrackApiClient TrackApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Track> _tracks = new();

    [Inject]
    private IPublisher Publisher { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        Publisher.Subscribe<Pilot>(this, x =>
        {
            return Task.CompletedTask;
        });

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