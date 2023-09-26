namespace EventStore.Helpers;
public interface IHandlerProvider
{
    Task Start();
    Task Stop();
}
