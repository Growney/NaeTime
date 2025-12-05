using NaeTime.Query.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class OpenPracticeEndpoints
{
    public static void MapOpenPracticeQueryHandlerEndpoints(this WebApplication app)
    {
        // GET /openpractice/session/{id}
        app.MapGet("/openpractice/session/{id:guid}", async (Guid id, IOpenPracticeQueryHandler handler) =>
        {
            var session = await handler.GetByIdAsync(id).ConfigureAwait(false);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        // GET /openpractice/session/{sessionId:guid}/track/{trackId:guid}/timing?minimumLapTime=00:00:01&maximumLapTime=00:01:00
        app.MapGet("/openpractice/session/{sessionId:guid}/track/{trackId:guid}/timing", async (Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime, IOpenPracticeQueryHandler handler) =>
        {
            var info = await handler.GetTimingInformation(sessionId, trackId, minimumLapTime, maximumLapTime).ConfigureAwait(false);
            return Results.Ok(info);
        });

        // GET /openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/timing?minimumLapTime=00:00:01&maximumLapTime=00:01:00
        app.MapGet("/openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/timing", async (Guid sessionId, Guid trackId, Guid pilotId, TimeSpan minimumLapTime, TimeSpan maximumLapTime, IOpenPracticeQueryHandler handler) =>
        {
            var info = await handler.GetTimingInformation(sessionId, trackId, pilotId, minimumLapTime, maximumLapTime).ConfigureAwait(false);
            return Results.Ok(info);
        });

        // GET /openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/lastdetection
        app.MapGet("/openpractice/session/{sessionId:guid}/track/{trackId:guid}/pilot/{pilotId:guid}/lastdetection", async (Guid sessionId, Guid trackId, Guid pilotId, IOpenPracticeQueryHandler handler) =>
        {
            var detection = await handler.GetPilotLastDetection(sessionId, trackId, pilotId).ConfigureAwait(false);
            return detection is null ? Results.NotFound() : Results.Ok(detection);
        });
    }
}
