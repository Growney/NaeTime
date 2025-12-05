namespace EventDbLite.Abstractions;
public class ReactionEvent<T>
{
    public ReactionEvent(T payload, SubscriptionEvent subscriptionEvent)
    {
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        SubscriptionEvent = subscriptionEvent ?? throw new ArgumentNullException(nameof(subscriptionEvent));
    }

    public T Payload { get; }
    public SubscriptionEvent SubscriptionEvent { get; }
}
