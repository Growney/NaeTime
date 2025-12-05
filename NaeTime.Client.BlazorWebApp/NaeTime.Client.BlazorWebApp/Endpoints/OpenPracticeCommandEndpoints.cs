using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class OpenPracticeCommandEndpoints
{
    public static void MapOpenPracticeCommandHandlerEndpoints(this WebApplication app)
    {
        app.MapPost("/openpractice/schedule", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid id, [FromQuery] Guid trackId, [FromQuery] string name) =>
        {
            await handler.ScheduleSession(id, trackId, name).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/clone", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid newId, [FromQuery] Guid existingId, [FromQuery] string newName) =>
        {
            await handler.CloneSession(newId, existingId, newName).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/clone-newtrack", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid newId, [FromQuery] Guid existingId, [FromQuery] string newName, [FromQuery] Guid trackId) =>
        {
            await handler.CloneSessionOnNewTrack(newId, existingId, newName, trackId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/rename", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] string name) =>
        {
            await handler.RenameSession(sessionId, name).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/disable-lane", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane) =>
        {
            await handler.DisableLane(sessionId, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/enable-lane", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane) =>
        {
            await handler.EnableLane(sessionId, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/tune-lane", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane, [FromQuery] int? bandId, [FromQuery] int frequencyInMhz) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.TuneLane(sessionId, (byte)lane, band, frequencyInMhz).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/set-lane-pilot", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane, [FromQuery] Guid pilotId) =>
        {
            await handler.SetLanePilot(sessionId, (byte)lane, pilotId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/reset-lane-pilot", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] int lane) =>
        {
            await handler.ResetLanePilot(sessionId, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/assign-detection", async (IOpenPracticeCommandHandler handler,
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

        app.MapPost("/openpractice/trigger-detection", async (IOpenPracticeCommandHandler handler,
        [FromQuery] Guid detectionId,
        [FromQuery] Guid sessionId,
        [FromQuery] int lane,
        [FromQuery] int ordinalPosition,
        [FromQuery] long? hardwareTime,
        [FromQuery] long softwareTime,
        [FromQuery] DateTime utcTime) =>
        {
            await handler.TriggerDetection(detectionId, sessionId, (byte)lane, (byte)ordinalPosition, hardwareTime == null ? null : (ulong?)hardwareTime.Value, softwareTime, utcTime).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/invalidate-detection", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InvalidateDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/invalidate-all-pilot-detections", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid pilotId) =>
        {
            await handler.InvalidateAllPilotDetections(sessionId, pilotId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/invalidate-pilot-detections-before", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InvalidatePilotDetectionsBeforeDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/validate-detection", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid detectionId, [FromQuery] Guid sessionId) =>
        {
            await handler.ValidateDetection(detectionId, sessionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/insert-pack-end-before", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InsertPilotPackEndBeforeDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/insert-pack-end-after", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid detectionId) =>
        {
            await handler.InsertPilotPackEndAfterDetection(sessionId, detectionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/openpractice/remove-pack-end", async (IOpenPracticeCommandHandler handler, [FromQuery] Guid sessionId, [FromQuery] Guid packEndId) =>
        {
            await handler.RemovePilotPackEnd(sessionId, packEndId).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
