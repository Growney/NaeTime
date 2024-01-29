namespace NaeTime.PubSub.Abstractions;
internal interface ISubscriptionHandler
{
    Task Handle(object message);
}
