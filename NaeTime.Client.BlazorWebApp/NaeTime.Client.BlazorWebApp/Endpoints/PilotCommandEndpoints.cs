using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class PilotCommandEndpoints
{
    public static void MapPilotCommandHandlerEndpoints(this WebApplication app)
    {
        app.MapPost("/pilot/create", async (IPilotCommandHandler handler, [FromQuery] Guid id, [FromQuery] string? firstName, [FromQuery] string? lastName, [FromQuery] string? callSign, [FromQuery] string? bindingPhrase) =>
        {
            await handler.CreatePilot(id, firstName, lastName, callSign, bindingPhrase).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/pilot/rename", async (IPilotCommandHandler handler, [FromQuery] Guid id, [FromQuery] string? firstName, [FromQuery] string? lastName) =>
        {
            await handler.RenamePilot(id, firstName, lastName).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/pilot/change-callsign", async (IPilotCommandHandler handler, [FromQuery] Guid id, [FromQuery] string? callSign) =>
        {
            await handler.ChangePilotCallsign(id, callSign).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/pilot/change-binding", async (IPilotCommandHandler handler, [FromQuery] Guid id, [FromQuery] string bindingPhrase) =>
        {
            await handler.ChangePilotBindingPhrase(id, bindingPhrase).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/pilot/remove-binding", async (IPilotCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.RemovePilotBindingPhrase(id).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
