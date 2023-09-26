using EventStore.Client;

namespace EventStore.Helpers;
public interface IProjectionBuilder
{
    void AddAllStreamProjectionHandler(FromAll position, Action<IProjectionHandler> configure);
    void AddStreamProjectionHandler(string streamName, FromStream position, Action<IProjectionHandler> configure);

}
