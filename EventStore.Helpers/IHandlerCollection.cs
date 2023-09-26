namespace EventStore.Helpers;
public interface IHandlerCollection
{
    void Add(Type eventType, Func<object, EventMetadata, Task> handler, string? streamName = null);
    IHandlerProvider BuildProvider();
}
