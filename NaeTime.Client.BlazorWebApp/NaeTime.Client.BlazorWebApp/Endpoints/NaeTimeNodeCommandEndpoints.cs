using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class NaeTimeNodeCommandEndpoints
{
    public static void MapNaeTimeNodeCommandHandlerEndpoints(this WebApplication app)
    {
        app.MapPost("/node/configure-serial-esp32", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name, [FromQuery] string port, [FromQuery] int lanes) =>
        {
            await handler.ConfigureSerialEsp32Node(id, name, port, (byte)lanes).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/reconfigure-serial", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string port) =>
        {
            await handler.ReconfigureSerialNode(id, port).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/rename", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name) =>
        {
            await handler.RenameDevice(id, name).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/request-enable-lane", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.RequestEnableLane(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/request-disable-lane", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.RequestDisableLane(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/confirm-enabled", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.ConfirmLaneEnabled(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/confirm-disabled", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.ConfirmLaneDisabled(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/request-frequency", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int? bandId, [FromQuery] int frequencyInMHz) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.RequestLaneFrequency(id, (byte)lane, band, frequencyInMHz).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/confirm-frequency", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int? bandId, [FromQuery] int frequencyInMHz) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.ConfirmLaneFrequencyTuned(id, (byte)lane, band, frequencyInMHz).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/request-entry-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.RequestLaneEntryThreshold(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/confirm-entry-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.ConfirmLaneEntryThresholdConfigured(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/request-exit-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.RequestLaneExitThreshold(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/confirm-exit-threshold", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int threshold) =>
        {
            await handler.ConfirmLaneExitThresholdConfigured(id, (byte)lane, (ushort)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/mark-lane-rf-setup-read", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] bool isEnabled, [FromQuery] int? bandId, [FromQuery] int frequencyInMHz, [FromQuery] int entryThreshold, [FromQuery] int exitThreshold) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.MarkLaneRFSetupRead(id, (byte)lane, isEnabled, band, frequencyInMHz, (ushort)entryThreshold, (ushort)exitThreshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/request-lane-rf-setup-confirmation", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.RequestLaneRFSetupConfirmation(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/mark-lane-rf-setup-confirmed", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.MarkLaneRFSetupConfirmed(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/mark-lane-rf-setup-mismatch", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.MarkLaneRFSetupMismatch(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/enable-lane-rf-setup-sync", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.EnableLaneRFSetupSync(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/disable-lane-rf-setup-sync", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane) =>
        {
            await handler.DisableLaneRFSetupSync(id, (byte)lane).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/request-timer-rf-setup-confirmation", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.RequestTimerRFSetupConfirmation(id).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/mark-timer-rf-setup-confirmed", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkTimerRFSetupConfirmed(id).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/mark-timer-rf-setup-mismatch", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkTimerRFSetupMismatch(id).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/mark-connected", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkAsConnected(id).ConfigureAwait(false);
            return Results.NoContent();
        });

        app.MapPost("/node/mark-disconnected", async (INaeTimeNodeCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkAsDisconnected(id).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
