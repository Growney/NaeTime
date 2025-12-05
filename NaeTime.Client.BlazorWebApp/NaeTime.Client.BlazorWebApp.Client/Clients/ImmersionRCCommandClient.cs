using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class ImmersionRCCommandClient : IImmersionRCLapRFCommandHandler
{
    private readonly HttpClient _http;

    public ImmersionRCCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task RegisterNetworkLapRF8Channel(Guid id, string name, IPAddress address, ushort port)
    {
        var url = $"/immersionrc/register?id={id}&name={Uri.EscapeDataString(name)}&address={Uri.EscapeDataString(address.ToString())}&port={port}";
        return PostNoContentAsync(url);
    }

    public Task ReconfigureNetworkDevice(Guid id, IPAddress address, ushort port)
    {
        var url = $"/immersionrc/reconfigure?id={id}&address={Uri.EscapeDataString(address.ToString())}&port={port}";
        return PostNoContentAsync(url);
    }

    public Task RenameDevice(Guid id, string name)
    {
        var url = $"/immersionrc/rename?id={id}&name={Uri.EscapeDataString(name)}";
        return PostNoContentAsync(url);
    }

    public Task SetupLaneForSession(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/immersionrc/setup-lane?id={id}&lane={lane}&isEnabled={isEnabled.ToString().ToLowerInvariant()}&frequencyInMHz={frequencyInMHz}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task ConfirmLaneSetup(Guid id, byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, float threshold, ushort gain)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/immersionrc/confirm-lane?id={id}&lane={lane}&isEnabled={isEnabled.ToString().ToLowerInvariant()}&frequencyInMHz={frequencyInMHz}&threshold={threshold.ToString(CultureInfo.InvariantCulture)}&gain={gain}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task RequestLaneStatus(Guid id, byte lane, bool isEnabled)
    {
        var url = $"/immersionrc/request-lane-status?id={id}&lane={lane}&isEnabled={isEnabled.ToString().ToLowerInvariant()}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneStatus(Guid id, byte lane, bool isEnabled)
    {
        var url = $"/immersionrc/confirm-lane-status?id={id}&lane={lane}&isEnabled={isEnabled.ToString().ToLowerInvariant()}";
        return PostNoContentAsync(url);
    }

    public Task RequestLaneFrequency(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/immersionrc/request-frequency?id={id}&lane={lane}&frequencyInMHz={frequencyInMHz}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task ConfirmLaneFrequencyTuned(Guid id, byte lane, byte? bandId, int frequencyInMHz)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/immersionrc/confirm-frequency?id={id}&lane={lane}&frequencyInMHz={frequencyInMHz}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task RequestLaneThreshold(Guid id, byte lane, float threshold)
    {
        var url = $"/immersionrc/request-threshold?id={id}&lane={lane}&threshold={threshold.ToString(CultureInfo.InvariantCulture)}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneThresholdConfigured(Guid id, byte lane, float threshold)
    {
        var url = $"/immersionrc/confirm-threshold?id={id}&lane={lane}&threshold={threshold.ToString(CultureInfo.InvariantCulture)}";
        return PostNoContentAsync(url);
    }

    public Task RequestLaneGain(Guid id, byte lane, ushort gain)
    {
        var url = $"/immersionrc/request-gain?id={id}&lane={lane}&gain={gain}";
        return PostNoContentAsync(url);
    }

    public Task ConfirmLaneGainConfigured(Guid id, byte lane, ushort gain)
    {
        var url = $"/immersionrc/confirm-gain?id={id}&lane={lane}&gain={gain}";
        return PostNoContentAsync(url);
    }

    public Task MarkAsConnected(Guid id)
    {
        var url = $"/immersionrc/mark-connected?id={id}";
        return PostNoContentAsync(url);
    }

    public Task MarkAsDisconnected(Guid id)
    {
        var url = $"/immersionrc/mark-disconnected?id={id}";
        return PostNoContentAsync(url);
    }
}
