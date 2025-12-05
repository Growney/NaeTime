using NaeTime.Query.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class TrackQueryHandlerEndPoints
{
    public static void MapTrackQueryHandlerEndpoints(this WebApplication app)
    {
        // GET /track/{id}
        app.MapGet("/track/{id:guid}", async (Guid id, ITrackQueryHandler handler) =>
        {
            var track = await handler.GetTrack(id).ConfigureAwait(false);
            return track is null ? Results.NotFound() : Results.Ok(track);
        });

        // GET /track/all
        app.MapGet("/track/all", async (ITrackQueryHandler handler) =>
        {
            var tracks = await handler.GetAllTracks().ConfigureAwait(false);
            return Results.Ok(tracks);
        });
    }
}
