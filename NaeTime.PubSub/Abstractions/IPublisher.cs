namespace NaeTime.PubSub.Abstractions;
public interface IPublisher
{
    void Subscribe<TMessageType>(object subscriber, Func<TMessageType, Task> onMessage);
    void RespondTo<TRequest, TResponse>(object subscriber, Func<TRequest, Task<TResponse?>> onRequest)
        where TResponse : class;

    void DisableSubscriber(Type subscriberType);
    void EnableSubscriber(Type subscriberType);
    void Unsubscribe(object subscriber);
}
