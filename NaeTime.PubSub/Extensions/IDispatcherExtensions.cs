using NaeTime.PubSub.Abstractions;

namespace NaeTime.PubSub.Extensions;
public static class IDispatcherExtensions
{
    public static async Task<TResponse?> RequestAsync<TRequest, TResponse>(this IDispatcher dispatcher, TRequest request)
        where TRequest : notnull
    {
        var subscriber = new object();
        TResponse? response = default;

        dispatcher.Publisher.Subscribe<TResponse>(subscriber, x =>
        {
            response = x;
            return Task.CompletedTask;
        });
        await dispatcher.Dispatch(request).ConfigureAwait(false);

        return response;
    }
}
