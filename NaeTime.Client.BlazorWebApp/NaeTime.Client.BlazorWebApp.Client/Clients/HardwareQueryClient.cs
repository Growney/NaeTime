using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using System.Net.Http.Json;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class HardwareQueryClient : IHardwareQueryHandler
{
    private readonly HttpClient _http;

    public HardwareQueryClient(HttpClient http)
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

    public async Task<Detector?> GetDetector(Guid id)
    {
        return await GetFromJsonOrNullAsync<Detector>($"/api/hardware/detector/{id}").ConfigureAwait(false);
    }

    public async Task<IEnumerable<Detector>> GetAllDetectors()
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<Detector>>("/api/hardware/detectors").ConfigureAwait(false);
        return result ?? Enumerable.Empty<Detector>();
    }

    public async Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids)
    {
        if (ids == null) return Enumerable.Empty<Detector>();
        var idsParam = string.Join(',', ids.Select(g => g.ToString()));
        var result = await GetFromJsonOrNullAsync<IEnumerable<Detector>>($"/api/hardware/detectors/ids?ids={Uri.EscapeDataString(idsParam)}").ConfigureAwait(false);
        return result ?? Enumerable.Empty<Detector>();
    }

    public async Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id)
    {
        return await GetFromJsonOrNullAsync<Ethernet8ChannelImmersionRCLapRF>($"/api/hardware/ethernetlaprf8channel/{id}").ConfigureAwait(false);
    }

    public async Task<ImmersionRCLapRFLane?> GetImmersionRCLapRFLane(Guid timerId, byte laneId)
    {
        return await GetFromJsonOrNullAsync<ImmersionRCLapRFLane>($"/api/hardware/immersionrclaprflane/{timerId}/{laneId}").ConfigureAwait(false);
    }
    public async Task<NetworkNaeTimeNode?> GetNetworkNaeTimeNode(Guid id)
    {
        return await GetFromJsonOrNullAsync<NetworkNaeTimeNode>($"/api/hardware/networknaetimenode/{id}").ConfigureAwait(false);
    }

    public async Task<IEnumerable<ImmersionRCLapRF>> GetAllImmersionRCLapRFs()
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<ImmersionRCLapRF>>("/api/hardware/immersionrclaprf/all").ConfigureAwait(false);
        return result ?? Enumerable.Empty<ImmersionRCLapRF>();
    }

    public async Task<IEnumerable<DesiredImmersionRCLapRFLane>> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId)
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<DesiredImmersionRCLapRFLane>>($"/api/hardware/immersionrclaprf/active/{timerId}").ConfigureAwait(false);
        return result ?? Enumerable.Empty<DesiredImmersionRCLapRFLane>();
    }

    public async Task<NaeTimeNodeLane?> GetNaeTimeNodeLane(Guid timerId, byte laneId)
    {
        return await GetFromJsonOrNullAsync<NaeTimeNodeLane>($"/api/hardware/naetimenode/{timerId}/lane/{laneId}").ConfigureAwait(false);
    }

    public async Task<IEnumerable<NaeTimeNode>> GetAllNaeTimeNodes()
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<NaeTimeNode>>("/api/hardware/naetimenode/all").ConfigureAwait(false);
        return result ?? Enumerable.Empty<NaeTimeNode>();
    }

    public async Task<IEnumerable<NetworkNaeTimeNode>> GetAllNetworkNaeTimeNodes()
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<NetworkNaeTimeNode>>("/api/hardware/networknaetimenode/all").ConfigureAwait(false);
        return result ?? Enumerable.Empty<NetworkNaeTimeNode>();
    }

    public async Task<IEnumerable<DesiredNaeTimeNodeLane>> GetActiveNaeTimeNodeLanesConfiguration(Guid timerId)
    {
        var result = await GetFromJsonOrNullAsync<IEnumerable<DesiredNaeTimeNodeLane>>($"/api/hardware/naetimenode/active/{timerId}").ConfigureAwait(false);
        return result ?? Enumerable.Empty<DesiredNaeTimeNodeLane>();
    }

    public async Task<TimerDetails> GetDetails(Guid timerId)
    {
        var res = await _http.GetAsync($"/api/hardware/timer/details/{timerId}").ConfigureAwait(false);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null!; // preserve original signature; caller should handle null if allowed

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TimerDetails>().ConfigureAwait(false)!;
    }

    public async Task<TimerLaneDetails> GetLaneDetails(Guid timerId, byte laneId)
    {
        var res = await _http.GetAsync($"/api/hardware/timer/{timerId}/lane/{laneId}/details").ConfigureAwait(false);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null!;

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TimerLaneDetails>().ConfigureAwait(false)!;
    }
}
