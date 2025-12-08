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
    }
}
