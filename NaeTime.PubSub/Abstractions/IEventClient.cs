namespace NaeTime.PubSub.Abstractions;
public interface IEventClient
{
    Task Publish(object obj);
}
