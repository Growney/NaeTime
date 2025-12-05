using EventDbLite.Reactions.SignalR.Server;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEventDbLiteSignalRServer(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddHttpClient();
        services.AddHostedService<EventHubService>();
        return services;
    }
}
