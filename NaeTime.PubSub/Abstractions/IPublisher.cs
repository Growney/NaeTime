namespace NaeTime.PubSub.Abstractions;
public interface IPublisher
{
    void Subscribe<TMessageType>(object subscriber, Func<TMessageType, Task> onMessage);
    void RespondTo<TRequest, TResponse>(object subscriber, Func<TRequest, Task<TResponse>> onRequest);
    void Unsubscribe<TMessageType>(object subscriber);
    void Unsubscribe(object subscriber);
}
