namespace NaeTime.PubSub.Abstractions;
public interface IDispatcher
{
    internal IPublisher Publisher { get; }

    Task Dispatch(object message);
}
