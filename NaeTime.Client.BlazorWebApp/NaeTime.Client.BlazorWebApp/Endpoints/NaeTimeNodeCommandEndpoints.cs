using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class NaeTimeNodeCommandEndpoints
{
    public static void MapNaeTimeNodeCommandHandlerEndpoints(this WebApplication app)
    {
        app.MapPost("/api/node/configure-serial-esp32", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name, [FromQuery] string port, [FromQuery] int lanes) =>
        {
            await handler.RegisterSerialEsp32Node(id, name, port, (byte)lanes).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/reconfigure-serial", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string port) =>
        {
            await handler.ChangeSerialEsp32Configuration(id, port).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/rename", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name) =>
        {
            await handler.RenameDevice(id, name).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/request-enable-lane", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.RequestLaneStatus(id, (byte)lane, true).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/request-disable-lane", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.RequestLaneStatus(id, (byte)lane, false).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/confirm-enabled", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.ConfirmLaneStatus(id, (byte)lane, true).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/confirm-disabled", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.ConfirmLaneStatus(id, (byte)lane, false).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/request-frequency", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int? bandId, [FromQuery] int frequencyInMHz) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.RequestLaneFrequency(id, (byte)lane, band, frequencyInMHz).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/confirm-frequency", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int? bandId, [FromQuery] int frequencyInMHz) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.ConfirmLaneFrequencyTuned(id, (byte)lane, band, frequencyInMHz).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/request-entry-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.RequestLaneEntryThreshold(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/confirm-entry-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.ConfirmLaneEntryThreshold(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/request-exit-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.RequestLaneExitThreshold(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/confirm-exit-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.ConfirmLaneExitThreshold(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/register-network", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name, [FromQuery] string ipAddress, [FromQuery] ushort port, [FromQuery] int lanes) =>
        {
            System.Net.IPAddress address = System.Net.IPAddress.Parse(ipAddress);
            await handler.RegisterNetworkNode(id, name, address, port, (byte)lanes).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/reconfigure-network", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string ipAddress, [FromQuery] ushort port) =>
        {
            System.Net.IPAddress address = System.Net.IPAddress.Parse(ipAddress);
            await handler.ReconfigureNetworkDevice(id, address, port).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/mark-connected", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkAsConnected(id).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/api/node/mark-disconnected", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkAsDisconnected(id).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
