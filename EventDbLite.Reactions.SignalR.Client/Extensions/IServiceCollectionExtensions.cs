using EventDbLite.Reactions.Abstractions;
using EventDbLite.Reactions.SignalR.Client;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEventDbSignalRReactions(this IServiceCollection services, string baseAddress)
    {
        services.AddSingleton(x => ActivatorUtilities.CreateInstance<SignalRReactionProviderFactory>(x, baseAddress));
        services.AddHostedService(x => x.GetRequiredService<SignalRReactionProviderFactory>());
        services.AddSingleton<IReactionProviderFactory>(x => x.GetRequiredService<SignalRReactionProviderFactory>());
        return services;
    }
}
