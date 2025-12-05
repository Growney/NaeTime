using EventDbLite.Abstractions;
using EventDbLite.Reactions.SignalR.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Routing;
public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapEventDbLiteService(this IEndpointRouteBuilder builder)
    {
        builder.MapHub<EventsHub>("/eventDbLiteHub");

        builder.MapGet("/events", async (HttpContext context, IEventStoreLite eventStore, long? position) =>
        {
            StreamPosition streamPosition = position.HasValue ? StreamPosition.WithVersion(position.Value) : StreamPosition.Beginning;

            List<StreamEvent> events = new();

            await foreach (var streamEvent in eventStore.ReadEvents(EventDbLite.Streams.StreamDirection.Forward, streamPosition))
            {
                events.Add(streamEvent);
            }

            return Results.Ok(events);
        });

        builder.MapGet("/events/{streamName}", async (HttpContext context, IEventStoreLite eventStore, string streamName, long? position) =>
        {
            StreamPosition streamPosition = position.HasValue ? StreamPosition.WithVersion(position.Value) : StreamPosition.Beginning;
            List<StreamEvent> events = new();
            await foreach (var streamEvent in eventStore.ReadStreamEvents(streamName, EventDbLite.Streams.StreamDirection.Forward, streamPosition))
            {
                events.Add(streamEvent);
            }
            return Results.Ok(events);
        });

        return builder;
    }
}
