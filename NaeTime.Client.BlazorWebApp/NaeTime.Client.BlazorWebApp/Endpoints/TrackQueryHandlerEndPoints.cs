using NaeTime.Query.Abstractions;

namespace Microsoft.AspNetCore.Builder;

public static class TrackQueryHandlerEndPoints
{
    public static void MapTrackQueryHandlerEndpoints(this WebApplication app)
    {
        // GET /api/track/{id}
        app.MapGet("/api/track/{id:guid}", async (Guid id, ITrackQueryHandler handler) =>
        {
            var track = await handler.GetTrack(id).ConfigureAwait(false);
            return track is null ? Results.NotFound() : Results.Ok(track);
        });

        // GET /api/track/all
        app.MapGet("/api/track/all", async (ITrackQueryHandler handler) =>
        {
            var tracks = await handler.GetAllTracks().ConfigureAwait(false);
            return Results.Ok(tracks);
        });
    }
}
