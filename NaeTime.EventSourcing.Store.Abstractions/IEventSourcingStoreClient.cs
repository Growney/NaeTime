namespace NaeTime.EventSourcing.Store.Abstractions;

public interface IEventSourcingStoreClient
{
    IAsyncEnumerable<EventData> GetEventsAsync(string streamId, long start);
    Task AppendEventAsync(string streamId, EventData eventData, long expectedVersion);
}
