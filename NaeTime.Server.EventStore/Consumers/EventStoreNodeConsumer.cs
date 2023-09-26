using EventStore.Client;
using EventStore.Helpers;
using NaeTime.Server.Abstractions.Consumers;
using NaeTime.Server.Abstractions.Events;

namespace NaeTime.Server.EventStore.Consumers;

public class EventStoreNodeConsumer : INodeConsumer
{
    private readonly EventStoreClient _client;

    public EventStoreNodeConsumer(EventStoreClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task When(RssiReadingGroupReceived readingGroup, IEnumerable<RssiReadingReceived> readings)
    {
        var eventData = new List<EventData>();

        eventData.Add(EventHelper.CreateEventData(readingGroup));

        foreach (var reading in readings)
        {
            eventData.Add(EventHelper.CreateEventData(reading));
        }

        var streamName = StreamHelper.GetStreamName("RssiStream", readingGroup.NodeId, readingGroup.DeviceId, readingGroup.FrequencyId);

        await _client.AppendToStreamAsync(streamName, StreamState.Any, eventData);
    }

}
