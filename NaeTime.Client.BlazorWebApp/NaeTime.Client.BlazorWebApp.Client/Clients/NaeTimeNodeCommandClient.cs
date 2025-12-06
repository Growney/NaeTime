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

    public Task ConfigureSerialEsp32Node(Guid id, string name, string port, byte lanes)
    {
        var url = $"/api/node/configure-serial-esp32?id={id}&name={Uri.EscapeDataString(name)}&port={Uri.EscapeDataString(port)}&lanes={lanes}";
        return PostNoContentAsync(url);
    }

    public Task ReconfigureSerialNode(Guid id, string port)
    {
        var url = $"/api/node/reconfigure-serial?id={id}&port={Uri.EscapeDataString(port)}";
        return PostNoContentAsync(url);
    }

    public Task RenameDevice(Guid id, string name)
    {
        var url = $"/api/node/rename?id={id}&name={Uri.EscapeDataString(name)}";
        return PostNoContentAsync(url);
    }

    public Task RequestEnableLane(Guid id, byte lane)
    {
        var url = $"/api/node/request-enable-lane?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task RequestDisableLane(Guid id, byte lane)
    {
        var url = $"/api/node/request-disable-lane?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneEnabled(Guid id, byte lane)
    {
        var url = $"/api/node/confirm-enabled?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneDisabled(Guid id, byte lane)
    {
        var url = $"/api/node/confirm-disabled?id={id}&lane={lane}";
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

    public Task ConfirmLaneEntryThresholdConfigured(Guid id, byte lane, ushort threshold)
    {
        var url = $"/api/node/confirm-entry-threshold?id={id}&lane={lane}&threshold={threshold}";
        return PostNoContentAsync(url);
    }

    public Task RequestLaneExitThreshold(Guid id, byte lane, ushort threshold)
    {
        var url = $"/api/node/request-exit-threshold?id={id}&lane={lane}&threshold={threshold}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneExitThresholdConfigured(Guid id, byte lane, ushort threshold)
    {
        var url = $"/api/node/confirm-exit-threshold?id={id}&lane={lane}&threshold={threshold}";
        return PostNoContentAsync(url);
    }

    public Task MarkLaneRFSetupRead(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, ushort entryThreshold, ushort exitThreshold)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/node/mark-lane-rf-setup-read?id={id}&lane={lane}&isEnabled={isEnabled.ToString().ToLowerInvariant()}&frequencyInMHz={frequencyInMHz}&entryThreshold={entryThreshold}&exitThreshold={exitThreshold}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task RequestLaneRFSetupConfirmation(Guid id, byte lane)
    {
        var url = $"/api/node/request-lane-rf-setup-confirmation?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task MarkLaneRFSetupConfirmed(Guid id, byte lane)
    {
        var url = $"/api/node/mark-lane-rf-setup-confirmed?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task MarkLaneRFSetupMismatch(Guid id, byte lane)
    {
        var url = $"/api/node/mark-lane-rf-setup-mismatch?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task EnableLaneRFSetupSync(Guid id, byte lane)
    {
        var url = $"/api/node/enable-lane-rf-setup-sync?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task DisableLaneRFSetupSync(Guid id, byte lane)
    {
        var url = $"/api/node/disable-lane-rf-setup-sync?id={id}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task RequestTimerRFSetupConfirmation(Guid id)
    {
        var url = $"/api/node/request-timer-rf-setup-confirmation?id={id}";
        return PostNoContentAsync(url);
    }

    public Task MarkTimerRFSetupConfirmed(Guid id)
    {
        var url = $"/api/node/mark-timer-rf-setup-confirmed?id={id}";
        return PostNoContentAsync(url);
    }

    public Task MarkTimerRFSetupMismatch(Guid id)
    {
        var url = $"/api/node/mark-timer-rf-setup-mismatch?id={id}";
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
}
