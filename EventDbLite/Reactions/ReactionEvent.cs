using EventDbLite.Streams;

namespace EventDbLite.Reactions;
public class ReactionEvent
{
    public ReactionEvent(object payload, SubscriptionEvent subscriptionEvent)
    {
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        SubscriptionEvent = subscriptionEvent ?? throw new ArgumentNullException(nameof(subscriptionEvent));
    }

    public object Payload { get; }
    public SubscriptionEvent SubscriptionEvent { get; }
}
