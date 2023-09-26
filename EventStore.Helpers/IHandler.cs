namespace EventStore.Helpers;
internal interface IHandler
{
    Task Start();
    Task Stop();

    void On(Type eventType, Func<object, EventMetadata, Task> handler);
}
