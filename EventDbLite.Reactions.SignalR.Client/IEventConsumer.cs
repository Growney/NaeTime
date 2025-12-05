using EventDbLite.Abstractions;

namespace EventDbLite.Reactions.SignalR.Client;
internal interface IEventConsumer
{
    public void AddEvent(StreamEvent streamEvent);
}
