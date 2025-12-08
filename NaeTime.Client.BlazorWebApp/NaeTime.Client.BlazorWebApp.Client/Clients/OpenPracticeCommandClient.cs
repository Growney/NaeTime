using System.Globalization;
using System.Net.Http.Json;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Client.Clients;

public class OpenPracticeCommandClient : IOpenPracticeCommandHandler
{
    private readonly HttpClient _http;

    public OpenPracticeCommandClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    private async Task PostNoContentAsync(string url)
    {
        using var res = await _http.PostAsync(url, null).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
    }

    public Task ScheduleSession(Guid id, Guid trackId, string name, TimeSpan? minimumLapTime, TimeSpan? maximumLapTime)
    {
        var url = $"/api/openpractice/schedule?id={id}&trackId={trackId}&name={Uri.EscapeDataString(name)}";
        if(minimumLapTime.HasValue)
            url += $"&minimumLapTimeInMs={minimumLapTime.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)}";
        if(maximumLapTime.HasValue)
            url += $"&maximumLapTimeInMs={maximumLapTime.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)}";

        return PostNoContentAsync(url);
    }

    public Task CloneSession(Guid newId, Guid existingId, string newName)
    {
        var url = $"/api/openpractice/clone?newId={newId}&existingId={existingId}&newName={Uri.EscapeDataString(newName)}";
        return PostNoContentAsync(url);
    }

    public Task CloneSessionOnNewTrack(Guid newId, Guid existingId, string newName, Guid trackId)
    {
        var url = $"/api/openpractice/clone-newtrack?newId={newId}&existingId={existingId}&newName={Uri.EscapeDataString(newName)}&trackId={trackId}";
        return PostNoContentAsync(url);
    }

    public Task RenameSession(Guid sessionId, string name)
    {
        var url = $"/api/openpractice/rename?sessionId={sessionId}&name={Uri.EscapeDataString(name)}";
        return PostNoContentAsync(url);
    }

    public Task DisableLane(Guid sessionId, byte lane)
    {
        var url = $"/api/openpractice/disable-lane?sessionId={sessionId}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task EnableLane(Guid sessionId, byte lane)
    {
        var url = $"/api/openpractice/enable-lane?sessionId={sessionId}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task TuneLane(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/openpractice/tune-lane?sessionId={sessionId}&lane={lane}&frequencyInMhz={frequencyInMhz}");
        if (bandId.HasValue) sb.Append($"&bandId={bandId.Value}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task SetLanePilot(Guid sessionId, byte lane, Guid pilotId)
    {
        var url = $"/api/openpractice/set-lane-pilot?sessionId={sessionId}&lane={lane}&pilotId={pilotId}";
        return PostNoContentAsync(url);
    }

    public Task ResetLanePilot(Guid sessionId, byte lane)
    {
        var url = $"/api/openpractice/reset-lane-pilot?sessionId={sessionId}&lane={lane}";
        return PostNoContentAsync(url);
    }

    public Task AssignHardwareDetectionToSession(Guid detectionId, Guid sessionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/openpractice/assign-detection?detectionId={detectionId}&sessionId={sessionId}&timerId={timerId}&lane={lane}");
        if (hardwareTime.HasValue) sb.Append($"&hardwareTime={hardwareTime.Value}");
        sb.Append($"&softwareTime={softwareTime}&utcTime={Uri.EscapeDataString(utcTime.ToString("o"))}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task TriggerDetection(Guid detectionId, Guid sessionId, byte lane, byte ordinalPosition)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"/api/openpractice/trigger-detection?detectionId={detectionId}&sessionId={sessionId}&lane={lane}&ordinalPosition={ordinalPosition}");
        return PostNoContentAsync(sb.ToString());
    }

    public Task InvalidateDetection(Guid sessionId, Guid detectionId)
    {
        var url = $"/api/openpractice/invalidate-detection?sessionId={sessionId}&detectionId={detectionId}";
        return PostNoContentAsync(url);
    }

    public Task InvalidateAllPilotDetections(Guid sessionId, Guid pilotId)
    {
        var url = $"/api/openpractice/invalidate-all-pilot-detections?sessionId={sessionId}&pilotId={pilotId}";
        return PostNoContentAsync(url);
    }

    public Task InvalidatePilotDetectionsBeforeDetection(Guid sessionId, Guid detectionId)
    {
        var url = $"/api/openpractice/invalidate-pilot-detections-before?sessionId={sessionId}&detectionId={detectionId}";
        return PostNoContentAsync(url);
    }

    public Task ValidateDetection(Guid detectionId, Guid sessionId)
    {
        var url = $"/api/openpractice/validate-detection?detectionId={detectionId}&sessionId={sessionId}";
        return PostNoContentAsync(url);
    }

    public Task InsertPilotPackEndBeforeDetection(Guid sessionId, Guid detectionId)
    {
        var url = $"/api/openpractice/insert-pack-end-before?sessionId={sessionId}&detectionId={detectionId}";
        return PostNoContentAsync(url);
    }

    public Task InsertPilotPackEndAfterDetection(Guid sessionId, Guid detectionId)
    {
        var url = $"/api/openpractice/insert-pack-end-after?sessionId={sessionId}&detectionId={detectionId}";
        return PostNoContentAsync(url);
    }

    public Task RemovePilotPackEnd(Guid sessionId, Guid packEndId)
    {
        var url = $"/api/openpractice/remove-pack-end?sessionId={sessionId}&packEndId={packEndId}";
        return PostNoContentAsync(url);
    }
}
