namespace NaeTime.PubSub.Abstractions;
public static class IPublisherExtensionMethods
{
    public static void EnableSubscriber<T>(this IPublisher publiser)
    {
        publiser.EnableSubscriber(typeof(T));
    }
    public static void DisableSubscriber<T>(this IPublisher publiser)
    {
        publiser.DisableSubscriber(typeof(T));
    }
}
