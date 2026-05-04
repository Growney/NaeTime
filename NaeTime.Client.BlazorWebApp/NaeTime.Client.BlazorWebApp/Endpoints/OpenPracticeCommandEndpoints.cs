using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class OpenPracticeCommandEndpoints
{
    public static void MapOpenPracticeCommandHandlerEndpoints(this WebApplication app)
    {
        app.MapPost("/api/openpractice/schedule", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid id, [FromQuery] Guid trackId, [FromQuery] string name, [FromQuery] double? minimumLapTimeInMs, [FromQuery] double? maximumLapTimeInMs) =>
        {
            TimeSpan? minimumLapTime = minimumLapTimeInMs.HasValue ? TimeSpan.FromMilliseconds(minimumLapTimeInMs.Value) : null;
            TimeSpan? maximumLapTime = maximumLapTimeInMs.HasValue ? TimeSpan.FromMilliseconds(maximumLapTimeInMs.Value) : null;
            await handler.ScheduleSession(id, trackId, name, minimumLapTime, maximumLapTime).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/clone", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid newId, [FromQuery] Guid existingId, [FromQuery] string newName) =>
        {
            await handler.CloneSession(newId, existingId, newName).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/clone-newtrack", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid newId, [FromQuery] Guid existingId, [FromQuery] string newName, [FromQuery] Guid trackId) =>
        {
            await handler.CloneSessionOnNewTrack(newId, existingId, newName, trackId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/rename", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] string name) =>
        {
            await handler.RenameSession(sessionId, name).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/disable-lane", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane) =>
        {
            await handler.DisableLane(sessionId, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/enable-lane", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane) =>
        {
            await handler.EnableLane(sessionId, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/tune-lane", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane, [FromQuery] int? bandId, [FromQuery] int frequencyInMhz) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.TuneLane(sessionId, (byte)lane, band, frequencyInMhz).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/set-lane-pilot", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane, [FromQuery] Guid pilotId) =>
        {
            await handler.SetLanePilot(sessionId, (byte)lane, pilotId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/reset-lane-pilot", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane) =>
        {
            await handler.ResetLanePilot(sessionId, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/assign-detection", async (IOpenPracticeCommandHandler handler,
        [FromQuery] Guid detectionId,
        [FromQuery] Guid sessionId,
        [FromQuery] Guid timerId,
        [FromQuery] int lane,
        [FromQuery] long? hardwareTime,
        [FromQuery] long softwareTime,
        [FromQuery] DateTime utcTime) =>
        {
            await handler.AssignHardwareDetectionToSession(detectionId, sessionId, timerId, (byte)lane, hardwareTime == null ? null : (ulong?)hardwareTime.Value, softwareTime, utcTime).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/trigger-detection", async (IOpenPracticeCommandHandler handler,
        [FromQuery] Guid detectionId,
        [FromQuery] Guid sessionId,
        [FromQuery] int lane,
        [FromQuery] int ordinalPosition) =>
        {
            await handler.TriggerDetection(detectionId, sessionId, (byte)lane, (byte)ordinalPosition).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/invalidate-detection", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InvalidateDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/invalidate-all-pilot-detections", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid pilotId) =>
        {
            await handler.InvalidateAllPilotDetections(sessionId, pilotId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/invalidate-pilot-detections-before", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InvalidatePilotDetectionsBeforeDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/validate-detection", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid detectionId, [FromQuery] Guid sessionId) =>
        {
            await handler.ValidateDetection(detectionId, sessionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/insert-pack-end-before", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InsertPilotPackEndBeforeDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/insert-pack-end-after", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InsertPilotPackEndAfterDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/remove-pack-end", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid packEndId) =>
        {
            await handler.RemovePilotPackEnd(sessionId, packEndId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/set-minimum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] double minimumLapTimeInMs) =>
        {
            await handler.SetMinimumLapTime(sessionId, TimeSpan.FromMilliseconds(minimumLapTimeInMs)).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/reset-minimum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId) =>
        {
            await handler.ResetMinimumLapTime(sessionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/set-maximum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] double maximumLapTimeInMs) =>
        {
            await handler.SetMaximumLapTime(sessionId, TimeSpan.FromMilliseconds(maximumLapTimeInMs)).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/reset-maximum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId) =>
        {
            await handler.ResetMaximumLapTime(sessionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/set-pilot-minimum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid pilotId, [FromQuery] double minimumLapTimeInMs) =>
        {
            await handler.SetPilotMinimumLapTime(sessionId, pilotId, TimeSpan.FromMilliseconds(minimumLapTimeInMs)).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/reset-pilot-minimum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid pilotId) =>
        {
            await handler.ResetPilotMinimumLapTime(sessionId, pilotId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/set-pilot-maximum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid pilotId, [FromQuery] double maximumLapTimeInMs) =>
        {
            await handler.SetPilotMaximumLapTime(sessionId, pilotId, TimeSpan.FromMilliseconds(maximumLapTimeInMs)).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/openpractice/reset-pilot-maximum-lap-time", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid pilotId) =>
        {
            await handler.ResetPilotMaximumLapTime(sessionId, pilotId).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
