using System.Net.Http.Json;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class SessionsCommandClient : ISessionsCommandHandler
{
    private readonly HttpClient _http;

    public SessionsCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task ActivateOpenPracticeSession(Guid id)
    {
        var url = $"/sessions/activate?id={id}";
        return PostNoContentAsync(url);
    }

    public Task DeactivateOpenPracticeSession(Guid id)
    {
        var url = $"/sessions/deactivate?id={id}";
        return PostNoContentAsync(url);
    }
}
