namespace NaeTime.PubSub.Abstractions;
internal interface IUniversalPublisher
{
    public Task Publish(object message);
}
