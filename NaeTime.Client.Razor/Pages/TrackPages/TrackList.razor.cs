using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class TrackList : IDisposable
{
    [Inject]
    private IPublishSubscribe PublishSubscribe { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Track> _tracks = new();

    protected override async Task OnInitializedAsync()
    {


        var tracksResponse = await PublishSubscribe.Request<TracksRequest, TracksResponse>();

        if (tracksResponse == null)
        {
            return;
        }

        foreach (var track in tracksResponse.Tracks)
        {
            var domainTrack = new Track()
            {
                Id = track.Id,
                Name = track.Name,
                MaximumLapTimeMilliseconds = track.MaximumLapTimeMilliseconds,
                MinimumLapTimeMilliseconds = track.MinimumLapTimeMilliseconds,
            };
            domainTrack.AddTimers(track.Timers);

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

    public void Dispose()
    {
        PublishSubscribe.Unsubscribe(this);
    }
}