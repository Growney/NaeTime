namespace EventStore.Helpers;
public interface IProjectionHandler
{
    IServiceProvider Services { get; }
    void On<T>(Func<T, EventMetadata, Task> onEvent);
    internal Task Start();
    internal Task Stop();
}
