namespace EventStore.Helpers;
public interface IProjection
{
    void Configure(IProjectionHandler handler);
}
