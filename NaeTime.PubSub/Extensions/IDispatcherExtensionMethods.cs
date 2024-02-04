namespace NaeTime.PubSub.Abstractions;
public static class IDispatcherExtensionMethods
{
    public static Task<TResponse?> Request<TRequest, TResponse>(this IDispatcher dispatcher)
        where TRequest : notnull, new()
    {
        var request = new TRequest();
        return dispatcher.Request<TRequest, TResponse>(request);
    }
}
