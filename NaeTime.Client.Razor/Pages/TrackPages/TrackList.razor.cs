using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Persistence.Abstractions.Management;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class TrackList
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Track> _tracks = new();

    protected override async Task OnInitializedAsync()
    {
        IEnumerable<Persistence.Abstractions.Management.Track>? tracksResponse = await RpcClient.InvokeAsync<IEnumerable<Persistence.Abstractions.Management.Track>>("GetTracks");

        if (tracksResponse == null)
        {
            return;
        }

        foreach (Persistence.Abstractions.Management.Track track in tracksResponse)
        {
            Track domainTrack = new()
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
}