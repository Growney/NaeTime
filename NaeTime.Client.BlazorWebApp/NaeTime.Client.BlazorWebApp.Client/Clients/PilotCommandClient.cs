using System.Net.Http.Json;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class PilotCommandClient : IPilotCommandHandler
{
    private readonly HttpClient _http;

    public PilotCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task CreatePilot(Guid id, string? firstName, string? lastName, string? callSign, string? bindingPhrase)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/pilot/create?id={id}");
        if (!string.IsNullOrEmpty(firstName)) sb.Append($"&firstName={Uri.EscapeDataString(firstName)}");
        if (!string.IsNullOrEmpty(lastName)) sb.Append($"&lastName={Uri.EscapeDataString(lastName)}");
        if (!string.IsNullOrEmpty(callSign)) sb.Append($"&callSign={Uri.EscapeDataString(callSign)}");
        if (!string.IsNullOrEmpty(bindingPhrase)) sb.Append($"&bindingPhrase={Uri.EscapeDataString(bindingPhrase)}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task RenamePilot(Guid id, string? firstName, string? lastName)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/pilot/rename?id={id}");
        if (!string.IsNullOrEmpty(firstName)) sb.Append($"&firstName={Uri.EscapeDataString(firstName)}");
        if (!string.IsNullOrEmpty(lastName)) sb.Append($"&lastName={Uri.EscapeDataString(lastName)}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task ChangePilotCallsign(Guid id, string? callSign)
    {
        var url = $"/api/pilot/change-callsign?id={id}";
        if (!string.IsNullOrEmpty(callSign)) url += $"&callSign={Uri.EscapeDataString(callSign)}";
        return PostNoContentAsync(url);
    }

    public Task ChangePilotBindingPhrase(Guid id, string bindingPhrase)
    {
        var url = $"/api/pilot/change-binding?id={id}&bindingPhrase={Uri.EscapeDataString(bindingPhrase)}";
        return PostNoContentAsync(url);
    }

    public Task RemovePilotBindingPhrase(Guid id)
    {
        var url = $"/api/pilot/remove-binding?id={id}";
        return PostNoContentAsync(url);
    }
}
