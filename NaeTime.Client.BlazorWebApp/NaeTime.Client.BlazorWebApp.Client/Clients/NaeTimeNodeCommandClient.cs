using System.Globalization;
using System.Net.Http.Json;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class NaeTimeNodeCommandClient : INaeTimeNodeCommandHandler
{
    private readonly HttpClient _http;

    public NaeTimeNodeCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task RegisterSerialEsp32Node(Guid id, string name, string port, byte lanes)
    {
        var url = $"/api/node/configure-serial-esp32?id={id}&name={Uri.EscapeDataString(name)}&port={Uri.EscapeDataString(port)}&lanes={lanes}";
        return PostNoContentAsync(url);
    }

    public Task ChangeSerialEsp32Configuration(Guid id, string port)
    {
        var url = $"/api/node/reconfigure-serial?id={id}&port={Uri.EscapeDataString(port)}";
        return PostNoContentAsync(url);
    }

    public Task RenameDevice(Guid id, string name)
    {
        var url = $"/api/node/rename?id={id}&name={Uri.EscapeDataString(name)}";
        return PostNoContentAsync(url);
    }

    public Task RequestLaneStatus(Guid id, byte lane, bool isEnabled)
    {
        var endpoint = isEnabled ? "/api/node/request-enable-lane" : "/api/node/request-disable-lane";
        var url = $"{endpoint}?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneStatus(Guid id, byte lane, bool isEnabled)
    {
        var endpoint = isEnabled ? "/api/node/confirm-enabled" : "/api/node/confirm-disabled";
        var url = $"{endpoint}?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/node/request-frequency?id={id}&lane={lane}&frequencyInMHz={frequencyInMHz}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/node/confirm-frequency?id={id}&lane={lane}&frequencyInMHz={frequencyInMHz}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task RequestLaneEntryThreshold(Guid id, byte lane, ushort threshold)
    {
        var url = $"/api/node/request-entry-threshold?id={id}&lane={lane}&threshold={threshold}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneEntryThreshold(Guid id, byte lane, ushort threshold)
    {
        var url = $"/api/node/confirm-entry-threshold?id={id}&lane={lane}&threshold={threshold}";
        return PostNoContentAsync(url);
    }

    public Task RequestLaneExitThreshold(Guid id, byte lane, ushort threshold)
    {
        var url = $"/api/node/request-exit-threshold?id={id}&lane={lane}&threshold={threshold}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneExitThreshold(Guid id, byte lane, ushort threshold)
    {
        var url = $"/api/node/confirm-exit-threshold?id={id}&lane={lane}&threshold={threshold}";
        return PostNoContentAsync(url);
    }

    public Task RegisterNetworkNode(Guid id, string name, System.Net.IPAddress address, ushort port, byte lanes)
    {
        var url = $"/api/node/register-network?id={id}&name={Uri.EscapeDataString(name)}&ipAddress={Uri.EscapeDataString(address.ToString())}&port={port}&lanes={lanes}";
        return PostNoContentAsync(url);
    }

    public Task ReconfigureNetworkDevice(Guid id, System.Net.IPAddress address, ushort port)
    {
        var url = $"/api/node/reconfigure-network?id={id}&ipAddress={Uri.EscapeDataString(address.ToString())}&port={port}";
        return PostNoContentAsync(url);
    }

    public Task MarkAsConnected(Guid id)
    {
        var url = $"/api/node/mark-connected?id={id}";
        return PostNoContentAsync(url);
    }

    public Task MarkAsDisconnected(Guid id)
    {
        var url = $"/api/node/mark-disconnected?id={id}";
        return PostNoContentAsync(url);
    }

    public Task SetupLaneForSession(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/node/setup-lane-for-session?id={id}&lane={lane}&isEnabled={isEnabled.ToString(CultureInfo.InvariantCulture)}&frequencyInMHz={frequencyInMHz}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task ConfirmLaneSetup(Guid timerId, byte laneId, bool isEnabled, byte? bandId, int frequencyInMHz, ushort entryThreshold, ushort exitThreshold)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/node/confirm-lane-setup?timerId={timerId}&laneId={laneId}&isEnabled={isEnabled.ToString(CultureInfo.InvariantCulture)}&frequencyInMHz={frequencyInMHz}&entryThreshold={entryThreshold}&exitThreshold={exitThreshold}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }
}
