using EventDbLite.Abstractions;
using EventDbLite.Handlers;
using EventDbLite.Handlers.Abstractions;
using EventDbLite.Reactions.Abstractions;
using EventDbLite.Reactions.SignalR.Client;
using EventDbLite.Serialization;
using Microsoft.Extensions.Logging;
namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEventDbSignalRReactions(this IServiceCollection services, Func<IServiceProvider, string> baseAddress)
    {
        services.AddScoped<IEventClient>(x =>
        {
            var logger = x.GetRequiredService<ILogger<EventClient>>();
            var client = new EventClient(logger, baseAddress(x));
            return client;
        });
        services.AddScoped<IReactionProviderFactory, SignalRReactionProviderFactory>();
        services.AddTransient<IEventSerializer, JsonEventSerializer>();
        services.AddTransient<IReactionClassFactory, ClientReactionClassFactory>();

        services.AddSingleton<IHandlerProvider, HandlerProvider>();
        services.AddSingleton<IAsyncHandlerProvider, AsyncHandlerProvider>();

        return services;
    }
}
