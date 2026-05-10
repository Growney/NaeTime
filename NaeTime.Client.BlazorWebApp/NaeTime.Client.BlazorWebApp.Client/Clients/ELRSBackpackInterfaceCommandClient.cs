using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class ELRSBackpackInterfaceCommandClient : IELRSBackpackInterfaceCommandHandler
{
    private readonly HttpClient _http;

    public ELRSBackpackInterfaceCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task Add(Guid id, string name, string comPort)
    {
        var url = $"/api/elrsbackpack/add?id={id}&name={Uri.EscapeDataString(name)}&comPort={Uri.EscapeDataString(comPort)}";
        return PostNoContentAsync(url);
    }

    public Task ReconfigureComPort(Guid id, string comPort)
    {
        var url = $"/api/elrsbackpack/reconfigure-comport?id={id}&comPort={Uri.EscapeDataString(comPort)}";
        return PostNoContentAsync(url);
    }

    public Task Rename(Guid id, string name)
    {
        var url = $"/api/elrsbackpack/rename?id={id}&name={Uri.EscapeDataString(name)}";
        return PostNoContentAsync(url);
    }
}
