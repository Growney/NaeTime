using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class ELRSBackpackInterfaceCommandEndPoints
{
    public static void MapELRSBackpackInterfaceCommandHandlerEndpoints(this WebApplication app)
    {
        // POST /api/elrsbackpack/add?id={id}&name={name}&comPort={comPort}
        app.MapPost("/api/elrsbackpack/add", async (IELRSBackpackInterfaceCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name, [FromQuery] string comPort) =>
        {
            await handler.Add(id, name, comPort).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/elrsbackpack/reconfigure-comport?id={id}&comPort={comPort}
        app.MapPost("/api/elrsbackpack/reconfigure-comport", async (IELRSBackpackInterfaceCommandHandler handler, [FromQuery] Guid id, [FromQuery] string comPort) =>
        {
            await handler.ReconfigureComPort(id, comPort).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/elrsbackpack/rename?id={id}&name={name}
        app.MapPost("/api/elrsbackpack/rename", async (IELRSBackpackInterfaceCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name) =>
        {
            await handler.Rename(id, name).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
