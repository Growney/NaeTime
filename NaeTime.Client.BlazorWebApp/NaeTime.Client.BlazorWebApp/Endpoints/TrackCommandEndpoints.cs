using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class TrackCommandEndpoints
{
    public static void MapTrackCommandHandlerEndpoints(this WebApplication app)
    {
        app.MapPost("/api/tracks/design", async (ITrackCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name, [FromQuery] Guid[] detectors) =>
        {
            await handler.DesignTrack(id, name, detectors).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/tracks/rename", async (ITrackCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name) =>
        {
            await handler.RenameTrack(id, name).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/tracks/reorder-detectors", async (ITrackCommandHandler handler, [FromQuery] Guid trackId, [FromQuery] Guid[] detectors) =>
        {
            await handler.ReorderTrackDetectors(trackId, detectors).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/tracks/set-max-lap", async (ITrackCommandHandler handler, [FromQuery] Guid trackId, [FromQuery] long milliseconds) =>
        {
            await handler.SetMaximumLapTime(trackId, milliseconds).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/tracks/set-min-detection-delay", async (ITrackCommandHandler handler, [FromQuery] Guid trackId, [FromQuery] long milliseconds) =>
        {
            await handler.SetMinimumDetectionDelay(trackId, milliseconds).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/tracks/reset-max-lap", async (ITrackCommandHandler handler, [FromQuery] Guid trackId) =>
        {
            await handler.ResetMaximumLapTime(trackId).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/tracks/reset-min-detection-delay", async (ITrackCommandHandler handler, [FromQuery] Guid trackId) =>
        {
            await handler.ResetMinimumDetectionDelay(trackId).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
