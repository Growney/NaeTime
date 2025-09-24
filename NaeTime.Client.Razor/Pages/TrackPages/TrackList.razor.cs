using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Query.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class TrackList
{
    [Inject]
    private ITrackQueryHandler TrackQueryHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Track> _tracks = new();

    protected override async Task OnInitializedAsync()
    {
        var tracks = await TrackQueryHandler.GetAllTracks();

        foreach (var track in tracks)
        {
            Track domainTrack = new()
            {
                Id = track.Id,
                Name = track.Name,
                MaximumLapTimeMilliseconds = track.MaximumLapTimeMilliseconds,
                MinimumLapTimeMilliseconds = track.MinimumLapTimeMilliseconds
            };
            domainTrack.AddTimers(track.Detectors.Select(x => x.Id));

            _tracks.Add(domainTrack);
        }

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