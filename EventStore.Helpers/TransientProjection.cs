namespace EventStore.Helpers;
public sealed class TransientProjection : IProjection, IProjectionHandler
{
    public TransientProjection(IServiceProvider services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IServiceProvider Services { get; }

    IServiceProvider IProjectionHandler.Services => throw new NotImplementedException();

    public void Configure(IProjectionHandler handler)
    {
    }

    public void On<T>(Func<T, EventMetadata, Task> onEvent)
    {
        throw new NotImplementedException();
    }

    void IProjectionHandler.On<T>(Func<T, EventMetadata, Task> onEvent)
    {
        throw new NotImplementedException();
    }

    Task IProjectionHandler.Start()
    {
        throw new NotImplementedException();
    }

    Task IProjectionHandler.Stop()
    {
        throw new NotImplementedException();
    }
}
