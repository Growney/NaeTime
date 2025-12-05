namespace EventDbLite.Abstractions;
public class SubscriptionEvent
{
    public SubscriptionEvent(bool isLive, StreamEvent @event)
    {
        IsLive = isLive;
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
    }

    public bool IsLive { get; }
    public StreamEvent Event { get; }
}
