using System.Net.Http.Json;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class TrackCommandClient : ITrackCommandHandler
{
    private readonly HttpClient _http;

    public TrackCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task DesignTrack(Guid id, string name, Guid[] detectors)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/tracks/design?id={id}&name={Uri.EscapeDataString(name)}");
        if (detectors != null && detectors.Length > 0)
        {
            // join as comma separated list
            var ids = string.Join(',', detectors.Select(g => g.ToString()));
            sb.Append($"&detectors={Uri.EscapeDataString(ids)}");
        }
        return PostNoContentAsync(sb.ToString());
    }

    public Task RenameTrack(Guid id, string name)
    {
        var url = $"/api/tracks/rename?id={id}&name={Uri.EscapeDataString(name)}";
        return PostNoContentAsync(url);
    }

    public Task ReorderTrackDetectors(Guid trackId, Guid[] detectors)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/tracks/reorder-detectors?trackId={trackId}");
        if (detectors != null && detectors.Length > 0)
        {
            var ids = string.Join(',', detectors.Select(g => g.ToString()));
            sb.Append($"&detectors={Uri.EscapeDataString(ids)}");
        }
        return PostNoContentAsync(sb.ToString());
    }
}
