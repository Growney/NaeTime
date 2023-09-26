using EventStore.Client;
using System.Text;
using System.Text.Json;

namespace EventStore.Helpers;

public static class EventHelper
{
    public static string GetEventTypeName(Type type) => type.FullName ?? type.Name;
    public static EventData CreateEventData(object eventObj)
    {
        var eventJson = System.Text.Json.JsonSerializer.Serialize(eventObj);
        var eventDataBytes = Encoding.UTF8.GetBytes(eventJson);

        var eventType = eventObj.GetType();

        var eventMetadata = new EventMetadata()
        {
            Created = DateTime.UtcNow,
            TypeName = GetEventTypeName(eventType)
        };

        var metadata = System.Text.Json.JsonSerializer.Serialize(eventMetadata);
        var metadataBytes = Encoding.UTF8.GetBytes(metadata);

        var eventDataObj = new EventData(Uuid.NewUuid(), eventMetadata.TypeName, eventDataBytes, metadataBytes);

        return eventDataObj;
    }
    public static object? CreateEvent(EventData data)
    {
        var eventType = Type.GetType(data.Type);

        if (eventType == null)
        {
            throw new InvalidOperationException("Event_Type_Not_Found");
        }

        var eventJson = Encoding.UTF8.GetString(data.Data.ToArray());

        var eventObj = JsonSerializer.Deserialize(eventJson, eventType);

        return eventObj;

    }
    public static object? CreateEvent(string eventTypeName, ReadOnlyMemory<byte> data)
    {
        var eventType = Type.GetType(eventTypeName);

        if (eventType == null)
        {
            throw new InvalidOperationException("Event_Type_Not_Found");
        }

        var eventJson = Encoding.UTF8.GetString(data.ToArray());

        var eventObj = JsonSerializer.Deserialize(eventJson, eventType);

        return eventObj;
    }
    public static EventMetadata? CreateEventMetadata(EventData data)
    {
        var metadataJson = Encoding.UTF8.GetString(data.Metadata.ToArray());

        return JsonSerializer.Deserialize<EventMetadata>(metadataJson);
    }
    public static EventMetadata? CreateEventMetadata(EventRecord record)
    {
        var metadataJson = Encoding.UTF8.GetString(record.Metadata.ToArray());

        return JsonSerializer.Deserialize<EventMetadata>(metadataJson);
    }

    internal static void AddHandlerFunction<T>(Dictionary<string, List<Func<object, EventMetadata, Task>>> handlers, Func<T, EventMetadata, Task> onEvent)
    {
        var name = EventHelper.GetEventTypeName(typeof(T));
        if (!handlers.TryGetValue(name, out var handlerList))
        {
            handlerList = new List<Func<object, EventMetadata, Task>>();
            handlers.Add(name, handlerList);
        }
        handlerList.Add((eventObject, eventMetadata) =>
        {
            var typedEvent = (T)eventObject;
            return onEvent(typedEvent, eventMetadata);
        });
    }
    internal static async Task OnEvent(Dictionary<string, List<Func<object, EventMetadata, Task>>> handlers, StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken token)
    {
        var metadata = EventHelper.CreateEventMetadata(resolvedEvent.Event);

        if (metadata != null && handlers.TryGetValue(metadata.TypeName, out var eventFunction))
        {
            var eventObject = EventHelper.CreateEvent(metadata.TypeName, resolvedEvent.Event.Data);

            if (eventObject != null)
            {
                foreach (var handler in handlers[metadata.TypeName])
                {
                    await handler(eventObject, metadata);
                }
            }
        }
    }
}
