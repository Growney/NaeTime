namespace NaeTime.PubSub.Abstractions;
public interface IEventClient
{
    Task PublishAsync(object obj);
}
