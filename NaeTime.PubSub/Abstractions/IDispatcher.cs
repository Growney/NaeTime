namespace NaeTime.PubSub.Abstractions;
public interface IDispatcher
{
    Task Dispatch(object message);
}
