using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class DetectionCommandClient : IDetectionCommandHandler
{
    private readonly HttpClient _http;

    public DetectionCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task Trigger(Guid id, Guid timerId, byte laneId, ulong hardwareTimer, long softwareTimer, DateTime utcTime)
    {
        var url = $"/api/detection/trigger?id={id}&timerId={timerId}&laneId={laneId}&hardwareTimer={hardwareTimer}&softwareTimer={softwareTimer}&utcTime={Uri.EscapeDataString(utcTime.ToString("O"))}";
        return PostNoContentAsync(url);
    }

    public Task Trigger(Guid id, Guid timerId, byte laneId)
    {
        var url = $"/api/detection/trigger-minimal?id={id}&timerId={timerId}&laneId={laneId}";
        return PostNoContentAsync(url);
    }

    public Task OverrideSession(Guid detectionId, Guid sessionId)
    {
        var url = $"/api/detection/override-session?detectionId={detectionId}&sessionId={sessionId}";
        return PostNoContentAsync(url);
    }

    public Task OverridePilot(Guid detectionId, Guid pilotId)
    {
        var url = $"/api/detection/override-pilot?detectionId={detectionId}&pilotId={pilotId}";
        return PostNoContentAsync(url);
    }

    public Task SetStatus(Guid detectionId, bool isValid)
    {
        var url = $"/api/detection/set-status?detectionId={detectionId}&isValid={isValid}";
        return PostNoContentAsync(url);
    }

    public Task Move(Guid detectionId, long softwareTime, DateTime utcTime)
    {
        var url = $"/api/detection/move?detectionId={detectionId}&softwareTime={softwareTime}&utcTime={Uri.EscapeDataString(utcTime.ToString("O"))}";
        return PostNoContentAsync(url);
    }
}
