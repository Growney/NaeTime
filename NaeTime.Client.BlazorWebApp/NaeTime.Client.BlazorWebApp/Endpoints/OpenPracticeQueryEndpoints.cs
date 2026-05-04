using NaeTime.Query.Abstractions;

namespace Microsoft.AspNetCore.Builder;

public static class OpenPracticeQueryEndpoints
{
    public static void MapOpenPracticeQueryHandlerEndpoints(this WebApplication app)
    {
        // GET /api/openpractice/session/{id}
        app.MapGet("/api/openpractice/session/{id:guid}", async (Guid id, IOpenPracticeQueryHandler handler) =>
        {
            var session = await handler.GetByIdAsync(id).ConfigureAwait(false);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        // GET /api/openpractice/session/{sessionId:guid}/track/{trackId:guid}/timing?minimumLapTime=00:00:01&maximumLapTime=00:01:00
        app.MapGet("/api/openpractice/session/{sessionId:guid}/track/{trackId:guid}/timing", async (Guid sessionId, Guid trackId, IOpenPracticeQueryHandler handler) =>
        {
            var info = await handler.GetTimingInformation(sessionId, trackId).ConfigureAwait(false);
            return Results.Ok(info);
        });

        // GET /api/openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/timing?minimumLapTime=00:00:01&maximumLapTime=00:01:00
        app.MapGet("/api/openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/timing", async (Guid sessionId, Guid trackId, Guid pilotId, IOpenPracticeQueryHandler handler) =>
        {
            var info = await handler.GetTimingInformation(sessionId, trackId, pilotId).ConfigureAwait(false);
            return Results.Ok(info);
        });

        // GET /api/openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/lastdetection
        app.MapGet("/api/openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/lastdetection", async (Guid sessionId, Guid trackId, Guid pilotId, IOpenPracticeQueryHandler handler) =>
        {
            var detection = await handler.GetPilotLastDetection(sessionId, trackId, pilotId).ConfigureAwait(false);
            return detection is null ? Results.NotFound() : Results.Ok(detection);
        });

        // GET /api/openpractice/session/{sessionId:guid}/pilot/{pilotId:guid}/lap-time-overrides
        app.MapGet("/api/openpractice/session/{sessionId:guid}/pilot/{pilotId:guid}/lap-time-overrides", async (Guid sessionId, Guid pilotId, IOpenPracticeQueryHandler handler) =>
        {
            var (min, max) = await handler.GetPilotLapTimeOverrides(sessionId, pilotId).ConfigureAwait(false);
            return Results.Ok(new { MinimumLapTimeMs = min?.TotalMilliseconds, MaximumLapTimeMs = max?.TotalMilliseconds });
        });
    }
}
