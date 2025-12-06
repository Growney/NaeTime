using Microsoft.AspNetCore.Builder;
using NaeTime.Query.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class PilotQueryHandlerEndpoints
{
 public static void MapPilotQueryHandlerEndpoints(this WebApplication app)
 {
 // GET /api/pilot/all
 app.MapGet("/api/pilot/all", async (IPilotQueryHandler handler) =>
 {
 var pilots = await handler.GetAllPilots().ConfigureAwait(false);
 return Results.Ok(pilots);
 });

 // GET /api/pilot/{id}
 app.MapGet("/api/pilot/{id:guid}", async (Guid id, IPilotQueryHandler handler) =>
 {
 var pilot = await handler.GetPilotById(id).ConfigureAwait(false);
 return pilot is null ? Results.NotFound() : Results.Ok(pilot);
 });
 }
}
