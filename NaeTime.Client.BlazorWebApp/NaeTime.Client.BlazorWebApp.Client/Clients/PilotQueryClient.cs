using System.Net.Http.Json;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class PilotQueryClient : IPilotQueryHandler
{
    private readonly HttpClient _http;

    public PilotQueryClient(HttpClient http)
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

    public async Task<IEnumerable<Pilot>> GetAllPilots()
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<Pilot>>("/pilot/all").ConfigureAwait(false);
        return result ?? Enumerable.Empty<Pilot>();
    }

    public async Task<Pilot?> GetPilotById(Guid id)
    {
        return await GetFromJsonOrNullAsync<Pilot>($"/pilot/{id}").ConfigureAwait(false);
    }
}
