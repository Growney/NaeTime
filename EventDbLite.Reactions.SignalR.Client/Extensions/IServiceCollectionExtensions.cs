using EventDbLite.Abstractions;
using EventDbLite.Reactions.Abstractions;
using EventDbLite.Reactions.SignalR.Client;
using EventDbLite.Serialization;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEventDbSignalRReactions(this IServiceCollection services, string baseAddress)
    {
        services.AddScoped(x => ActivatorUtilities.CreateInstance<SignalRReactionProviderFactory>(x, baseAddress));
        services.AddScoped<IReactionProviderFactory>(x => x.GetRequiredService<SignalRReactionProviderFactory>());
        services.AddTransient<IEventSerializer, JsonEventSerializer>();
        return services;
    }
}
