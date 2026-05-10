using NaeTime.Query.Abstractions;

namespace Microsoft.AspNetCore.Builder;
public static class HardwareQueryEndpoints
{
    // Call app.MapQueryHandlerEndpoints() in __Program.cs__
    public static void MapHardwareQueryHandlerEndpoints(this WebApplication app)
    {
        // GET api/hardware/detector/{id}
        app.MapGet("/api/hardware/detector/{id:guid}", async (Guid id, IHardwareQueryHandler handler) =>
        {
            var detector = await handler.GetDetector(id).ConfigureAwait(false);
            return detector is null ? Results.NotFound() : Results.Ok(detector);
        });

        // GET api/hardware/detectors
        app.MapGet("/api/hardware/detectors", async (IHardwareQueryHandler handler) =>
        {
            var detectors = await handler.GetAllDetectors().ConfigureAwait(false);
            return Results.Ok(detectors);
        });

        // GET api/hardware/detectors/ids?ids=guid1,guid2,...
        app.MapGet("/api/hardware/detectors/ids", async (HttpRequest req, IHardwareQueryHandler handler) =>
        {
            if (!req.Query.TryGetValue("ids", out var idsValues) || string.IsNullOrWhiteSpace(idsValues))
            {
                return Results.BadRequest("Query parameter 'ids' is required (comma separated GUIDs).");
            }

            Guid[] ids;
            try
            {
                ids = idsValues.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Guid.Parse(s.Trim()))
                    .ToArray();
            }
            catch (FormatException)
            {
                return Results.BadRequest("One or more ids are not valid GUIDs.");
            }

            var result = await handler.GetDetectors(ids).ConfigureAwait(false);
            return Results.Ok(result);
        });

        // GET api/hardware/ethernetlaprf8channel/{id}
        app.MapGet("/api/hardware/ethernetlaprf8channel/{id:guid}", async (Guid id, IHardwareQueryHandler handler) =>
        {
            var rf = await handler.GetEthernet8ChannelImmersionRCLapRF(id).ConfigureAwait(false);
            return rf is null ? Results.NotFound() : Results.Ok(rf);
        });

        app.MapGet("/api/hardware/networknaetimenode/{id:guid}", async (Guid id, IHardwareQueryHandler handler) =>
        {
            var node = await handler.GetNetworkNaeTimeNode(id).ConfigureAwait(false);
            return node is null ? Results.NotFound() : Results.Ok(node);
        });

        // GET api/hardware/naetimenode/all
        app.MapGet("/api/hardware/naetimenode/all", async (IHardwareQueryHandler handler) =>
        {
            var nodes = await handler.GetAllNaeTimeNodes().ConfigureAwait(false);
            return Results.Ok(nodes);
        });

        // GET api/hardware/naetimenode/{timerId}/lane/{laneId}
        app.MapGet("/api/hardware/naetimenode/{timerId:guid}/lane/{laneId:int}", async (Guid timerId, int laneId, IHardwareQueryHandler handler) =>
        {
            if (laneId < byte.MinValue || laneId > byte.MaxValue)
            {
                return Results.BadRequest($"laneId must be between {byte.MinValue} and {byte.MaxValue}.");
            }

            var lane = await handler.GetNaeTimeNodeLane(timerId, (byte)laneId).ConfigureAwait(false);
            return lane is null ? Results.NotFound() : Results.Ok(lane);
        });

        // GET api/hardware/naetimenode/active/{timerId}
        app.MapGet("/api/hardware/naetimenode/active/{timerId:guid}", async (Guid timerId, IHardwareQueryHandler handler) =>
        {
            var config = await handler.GetActiveNaeTimeNodeLanesConfiguration(timerId).ConfigureAwait(false);
            return Results.Ok(config);
        });

        // GET api/hardware/immersionrclaprflane/{timerId}/{laneId}
        app.MapGet("/api/hardware/immersionrclaprflane/{timerId:guid}/{laneId:int}", async (Guid timerId, int laneId, IHardwareQueryHandler handler) =>
        {
            if (laneId < byte.MinValue || laneId > byte.MaxValue)
            {
                return Results.BadRequest($"laneId must be between {byte.MinValue} and {byte.MaxValue}.");
            }

            var lane = await handler.GetImmersionRCLapRFLane(timerId, (byte)laneId).ConfigureAwait(false);
            return lane is null ? Results.NotFound() : Results.Ok(lane);
        });

        // GET api/hardware/immersionrclaprf/all
        app.MapGet("/api/hardware/immersionrclaprf/all", async (IHardwareQueryHandler handler) =>
        {
            var list = await handler.GetAllImmersionRCLapRFs().ConfigureAwait(false);
            return Results.Ok(list);
        });

        // GET api/hardware/immersionrclaprf/active/{timerId}
        app.MapGet("/api/hardware/immersionrclaprf/active/{timerId:guid}", async (Guid timerId, IHardwareQueryHandler handler) =>
        {
            var config = await handler.GetActiveImmersionRCLapRFLanesConfiguration(timerId).ConfigureAwait(false);
            return Results.Ok(config);
        });

        // GET api/hardware/timer/details/{timerId}
        app.MapGet("/api/hardware/timer/details/{timerId:guid}", async (Guid timerId, IHardwareQueryHandler handler) =>
        {
            var details = await handler.GetDetails(timerId).ConfigureAwait(false);
            return details is null ? Results.NotFound() : Results.Ok(details);
        });

        // GET api/hardware/timer/{timerId}/lane/{laneId}/details
        app.MapGet("/api/hardware/timer/{timerId:guid}/lane/{laneId:int}/details", async (Guid timerId, int laneId, IHardwareQueryHandler handler) =>
        {
            if (laneId < byte.MinValue || laneId > byte.MaxValue)
            {
                return Results.BadRequest($"laneId must be between {byte.MinValue} and {byte.MaxValue}.");
            }

            var laneDetails = await handler.GetLaneDetails(timerId, (byte)laneId).ConfigureAwait(false);
            return laneDetails is null ? Results.NotFound() : Results.Ok(laneDetails);
        });

        // GET api/hardware/interface/{id}
        app.MapGet("/api/hardware/interface/{id:guid}", async (Guid id, IHardwareQueryHandler handler) =>
        {
            var iface = await handler.GetInterface(id).ConfigureAwait(false);
            return iface is null ? Results.NotFound() : Results.Ok(iface);
        });

        // GET api/hardware/interfaces
        app.MapGet("/api/hardware/interfaces", async (IHardwareQueryHandler handler) =>
        {
            var interfaces = await handler.GetAllInterfaces().ConfigureAwait(false);
            return Results.Ok(interfaces);
        });

        // GET api/hardware/interfaces/ids?ids=guid1,guid2,...
        app.MapGet("/api/hardware/interfaces/ids", async (HttpRequest req, IHardwareQueryHandler handler) =>
        {
            if (!req.Query.TryGetValue("ids", out var idsValues) || string.IsNullOrWhiteSpace(idsValues))
            {
                return Results.BadRequest("Query parameter 'ids' is required (comma separated GUIDs).");
            }

            Guid[] ids;
            try
            {
                ids = idsValues.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Guid.Parse(s.Trim()))
                    .ToArray();
            }
            catch (FormatException)
            {
                return Results.BadRequest("One or more ids are not valid GUIDs.");
            }

            var result = await handler.GetInterfaces(ids).ConfigureAwait(false);
            return Results.Ok(result);
        });

        // GET api/hardware/elrsbackpackinterface/{id}
        app.MapGet("/api/hardware/elrsbackpackinterface/{id:guid}", async (Guid id, IHardwareQueryHandler handler) =>
        {
            var backpack = await handler.GetSerialELRSBackpackInterface(id).ConfigureAwait(false);
            return backpack is null ? Results.NotFound() : Results.Ok(backpack);
        });

        // GET api/hardware/elrsbackpackinterface/all
        app.MapGet("/api/hardware/elrsbackpackinterface/all", async (IHardwareQueryHandler handler) =>
        {
            var backpacks = await handler.GetAllSerialELRSBackpackInterfaces().ConfigureAwait(false);
            return Results.Ok(backpacks);
        });
    }
}