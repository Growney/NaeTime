using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class DetectionCommandEndPoints
{
    public static void MapDetectionCommandHandlerEndpoints(this WebApplication app)
    {
        // POST /api/detection/trigger?id={id}&timerId={timerId}&laneId={laneId}&hardwareTimer={hardwareTimer}&softwareTimer={softwareTimer}&utcTime={utcTime}
        app.MapPost("/api/detection/trigger", async (IDetectionCommandHandler handler, [FromQuery] Guid id, [FromQuery] Guid timerId, [FromQuery] byte laneId, [FromQuery] ulong hardwareTimer, [FromQuery] long softwareTimer, [FromQuery] DateTime utcTime) =>
        {
            await handler.Trigger(id, timerId, laneId, hardwareTimer, softwareTimer, utcTime).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/detection/trigger-minimal?id={id}&timerId={timerId}&laneId={laneId}
        app.MapPost("/api/detection/trigger-minimal", async (IDetectionCommandHandler handler, [FromQuery] Guid id, [FromQuery] Guid timerId, [FromQuery] byte laneId) =>
        {
            await handler.Trigger(id, timerId, laneId).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/detection/override-session?detectionId={detectionId}&sessionId={sessionId}
        app.MapPost("/api/detection/override-session", async (IDetectionCommandHandler handler, [FromQuery] Guid detectionId, [FromQuery] Guid sessionId) =>
        {
            await handler.OverrideSession(detectionId, sessionId).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/detection/override-pilot?detectionId={detectionId}&pilotId={pilotId}
        app.MapPost("/api/detection/override-pilot", async (IDetectionCommandHandler handler, [FromQuery] Guid detectionId, [FromQuery] Guid pilotId) =>
        {
            await handler.OverridePilot(detectionId, pilotId).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/detection/set-status?detectionId={detectionId}&isValid={isValid}
        app.MapPost("/api/detection/set-status", async (IDetectionCommandHandler handler, [FromQuery] Guid detectionId, [FromQuery] bool isValid) =>
        {
            await handler.SetStatus(detectionId, isValid).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/detection/move?detectionId={detectionId}&softwareTime={softwareTime}&utcTime={utcTime}
        app.MapPost("/api/detection/move", async (IDetectionCommandHandler handler, [FromQuery] Guid detectionId, [FromQuery] long softwareTime, [FromQuery] DateTime utcTime) =>
        {
            await handler.Move(detectionId, softwareTime, utcTime).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
