using System.Net.Http.Json;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class SessionQueryClient : ISessionQueryHandler
{
    private readonly HttpClient _http;

    public SessionQueryClient(HttpClient http)
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

    public async Task<Session?> GetSession(Guid id)
    {
        return await GetFromJsonOrNullAsync<Session>($"/api/session/{id}").ConfigureAwait(false);
    }

    public async Task<IEnumerable<Session>> GetAllSessions()
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<Session>>("/api/session/all").ConfigureAwait(false);
        return result ?? Enumerable.Empty<Session>();
    }

    public async Task<Session?> GetActiveSession()
    {
        return await GetFromJsonOrNullAsync<Session>("/api/session/active").ConfigureAwait(false);
    }
}
