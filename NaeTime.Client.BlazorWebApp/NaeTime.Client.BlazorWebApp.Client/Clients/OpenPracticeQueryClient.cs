using System.Net.Http.Json;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using System.Collections.Generic;
using System.Linq;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class OpenPracticeQueryClient : IOpenPracticeQueryHandler
{
    private readonly HttpClient _http;

    public OpenPracticeQueryClient(HttpClient http)
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

    public async Task<OpenPracticeSession?> GetByIdAsync(Guid id)
    {
        return await GetFromJsonOrNullAsync<OpenPracticeSession>($"/api/openpractice/session/{id}").ConfigureAwait(false);
    }

    public async Task<SessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId)
    {
        var url = $"/api/openpractice/session/{sessionId}/track/{trackId}/timing";
        var result = await GetFromJsonOrNullAsync<SessionTimingInformation>(url).ConfigureAwait(false);
        if (result != null) return result;

        return new SessionTimingInformation(
            new Dictionary<Guid, IEnumerable<TimingMoment>>(),
            new Dictionary<Guid, IDictionary<Guid, Detection>>(),
            new Dictionary<Guid, IEnumerable<IEnumerable<OpenPracticeLap>>>(),
            new Dictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>>(),
            new Dictionary<uint, IEnumerable<OpenPracticeLapRecord>>()
        );
    }

    public async Task<SessionPilotTimingInfo> GetTimingInformation(Guid sessionId, Guid trackId, Guid pilotId)
    {
        var url = $"/api/openpractice/session/{sessionId}/track/{trackId}/pilot/{pilotId}/timing";
        var result = await GetFromJsonOrNullAsync<SessionPilotTimingInfo>(url).ConfigureAwait(false);
        if (result != null) return result;

        return new SessionPilotTimingInfo(
            Enumerable.Empty<TimingMoment>(),
            new Dictionary<Guid, Detection>(),
            Enumerable.Empty<IEnumerable<OpenPracticeLap>>(),
            new Dictionary<uint, OpenPracticeLapRecord>()
        );
    }

    public async Task<Detection?> GetPilotLastDetection(Guid sessionId, Guid trackId, Guid pilotId)
    {
        return await GetFromJsonOrNullAsync<Detection>($"/api/openpractice/session/{sessionId}/track/{trackId}/pilot/{pilotId}/lastdetection").ConfigureAwait(false);
    }

    private record PilotLapTimeOverridesDto(double? MinimumLapTimeMs, double? MaximumLapTimeMs);

    public async Task<(TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime)> GetPilotLapTimeOverrides(Guid sessionId, Guid pilotId)
    {
        var dto = await GetFromJsonOrNullAsync<PilotLapTimeOverridesDto>($"/api/openpractice/session/{sessionId}/pilot/{pilotId}/lap-time-overrides").ConfigureAwait(false);
        if (dto == null) return (null, null);
        return (dto.MinimumLapTimeMs.HasValue ? TimeSpan.FromMilliseconds(dto.MinimumLapTimeMs.Value) : null,
                dto.MaximumLapTimeMs.HasValue ? TimeSpan.FromMilliseconds(dto.MaximumLapTimeMs.Value) : null);
    }
}
