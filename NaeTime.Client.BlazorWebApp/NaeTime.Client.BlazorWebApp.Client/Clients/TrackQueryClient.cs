using System.Net.Http.Json;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class TrackQueryClient : ITrackQueryHandler
{
    private readonly HttpClient _http;

    public TrackQueryClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task<T?> GetFromJsonOrNullAsync<T>(string url)
    {
        using var res = await _http.GetAsync(url).ConfigureAwait(false);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
    }

    public async Task<Track?> GetTrack(Guid id)
    {
        return await GetFromJsonOrNullAsync<Track>($"/track/{id}").ConfigureAwait(false);
    }

    public async Task<IEnumerable<Track>> GetAllTracks()
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<Track>>("/track/all").ConfigureAwait(false);
        return result ?? Enumerable.Empty<Track>();
    }
}
