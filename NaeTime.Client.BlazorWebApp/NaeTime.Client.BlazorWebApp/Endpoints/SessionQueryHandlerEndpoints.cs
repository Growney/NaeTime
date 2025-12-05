using NaeTime.Query.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class SessionQueryHandlerEndpoints
{
    public static void MapSessionQueryHandlerEndpoints(this WebApplication app)
    {
        // GET /session/{id}
        app.MapGet("/session/{id:guid}", async (Guid id, ISessionQueryHandler handler) =>
        {
            var session = await handler.GetSession(id).ConfigureAwait(false);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        // GET /session/all
        app.MapGet("/session/all", async (ISessionQueryHandler handler) =>
        {
            var sessions = await handler.GetAllSessions().ConfigureAwait(false);
            return Results.Ok(sessions);
        });

        // GET /session/active
        app.MapGet("/session/active", async (ISessionQueryHandler handler) =>
        {
            var active = await handler.GetActiveSession().ConfigureAwait(false);
            return active is null ? Results.NotFound() : Results.Ok(active);
        });
    }
}
