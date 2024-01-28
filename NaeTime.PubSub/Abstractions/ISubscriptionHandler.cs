namespace NaeTime.PubSub.Abstractions;
public interface ISubscriptionHandler
{
    Task Handle(object message);
}
