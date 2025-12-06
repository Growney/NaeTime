using System.Net;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class ImmersionRCCommandEndpoints
{
    public static void MapImmersionRCCommandHandlerEndpoints(this WebApplication app)
    {
        // POST /api/immersionrc/register?id={id}&name={name}&address={address}&port={port}
        app.MapPost("/api/immersionrc/register", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name, [FromQuery] string address, [FromQuery] int port) =>
        {
            await handler.RegisterNetworkLapRF8Channel(id, name, IPAddress.Parse(address), (ushort)port).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/reconfigure?id={id}&address={address}&port={port}
        app.MapPost("/api/immersionrc/reconfigure", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] string address, [FromQuery] int port) =>
        {
            await handler.ReconfigureNetworkDevice(id, IPAddress.Parse(address), (ushort)port).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/rename?id={id}&name={name}
        app.MapPost("/api/immersionrc/rename", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] string name) =>
        {
            await handler.RenameDevice(id, name).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/setup-lane?id={id}&lane={lane}&isEnabled={isEnabled}&frequencyInMHz={frequencyInMHz}&bandId={bandId}
        app.MapPost("/api/immersionrc/setup-lane", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] bool isEnabled, [FromQuery] int frequencyInMHz, [FromQuery] int? bandId) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.SetupLaneForSession(id, (byte)lane, isEnabled, band, frequencyInMHz).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/confirm-lane?id={id}&lane={lane}&isEnabled={isEnabled}&frequencyInMHz={frequencyInMHz}&threshold={threshold}&gain={gain}&bandId={bandId}
        app.MapPost("/api/immersionrc/confirm-lane", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] bool isEnabled, [FromQuery] int frequencyInMHz, [FromQuery] double threshold, [FromQuery] int gain, [FromQuery] int? bandId) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.ConfirmLaneSetup(id, (byte)lane, isEnabled, band, frequencyInMHz, (float)threshold, (ushort)gain).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/request-lane-status?id={id}&lane={lane}&isEnabled={isEnabled}
        app.MapPost("/api/immersionrc/request-lane-status", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] bool isEnabled) =>
        {
            await handler.RequestLaneStatus(id, (byte)lane, isEnabled).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/confirm-lane-status?id={id}&lane={lane}&isEnabled={isEnabled}
        app.MapPost("/api/immersionrc/confirm-lane-status", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] bool isEnabled) =>
        {
            await handler.ConfirmLaneStatus(id, (byte)lane, isEnabled).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/request-frequency?id={id}&lane={lane}&frequencyInMHz={frequencyInMHz}&bandId={bandId}
        app.MapPost("/api/immersionrc/request-frequency", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int frequencyInMHz, [FromQuery] int? bandId) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.RequestLaneFrequency(id, (byte)lane, band, frequencyInMHz).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/confirm-frequency?id={id}&lane={lane}&frequencyInMHz={frequencyInMHz}&bandId={bandId}
        app.MapPost("/api/immersionrc/confirm-frequency", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int frequencyInMHz, [FromQuery] int? bandId) =>
        {
            byte? band = bandId.HasValue ? (byte?)bandId.Value : null;
            await handler.ConfirmLaneFrequencyTuned(id, (byte)lane, band, frequencyInMHz).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/request-threshold?id={id}&lane={lane}&threshold={threshold}
        app.MapPost("/api/immersionrc/request-threshold", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] double threshold) =>
        {
            await handler.RequestLaneThreshold(id, (byte)lane, (float)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/confirm-threshold?id={id}&lane={lane}&threshold={threshold}
        app.MapPost("/api/immersionrc/confirm-threshold", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] double threshold) =>
        {
            await handler.ConfirmLaneThresholdConfigured(id, (byte)lane, (float)threshold).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/request-gain?id={id}&lane={lane}&gain={gain}
        app.MapPost("/api/immersionrc/request-gain", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int gain) =>
        {
            await handler.RequestLaneGain(id, (byte)lane, (ushort)gain).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/confirm-gain?id={id}&lane={lane}&gain={gain}
        app.MapPost("/api/immersionrc/confirm-gain", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id, [FromQuery] int lane, [FromQuery] int gain) =>
        {
            await handler.ConfirmLaneGainConfigured(id, (byte)lane, (ushort)gain).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/mark-connected?id={id}
        app.MapPost("/api/immersionrc/mark-connected", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkAsConnected(id).ConfigureAwait(false);
            return Results.NoContent();
        });

        // POST /api/immersionrc/mark-disconnected?id={id}
        app.MapPost("/api/immersionrc/mark-disconnected", async (IImmersionRCLapRFCommandHandler handler, [FromQuery] Guid id) =>
        {
            await handler.MarkAsDisconnected(id).ConfigureAwait(false);
            return Results.NoContent();
        });
    }
}
