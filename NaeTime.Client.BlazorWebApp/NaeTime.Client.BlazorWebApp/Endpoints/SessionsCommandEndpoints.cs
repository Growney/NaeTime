using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class SessionsCommandEndpoints
{
 public static void MapSessionsCommandHandlerEndpoints(this WebApplication app)
 {
 app.MapPost("/api/sessions/activate", async (ISessionsCommandHandler handler, [FromQuery] Guid id) =>
 {
 await handler.ActivateOpenPracticeSession(id).ConfigureAwait(false);
 return Results.NoContent();
 });

 app.MapPost("/api/sessions/deactivate", async (ISessionsCommandHandler handler, [FromQuery] Guid id) =>
 {
 await handler.DeactivateOpenPracticeSession(id).ConfigureAwait(false);
 return Results.NoContent();
 });
 }
}
